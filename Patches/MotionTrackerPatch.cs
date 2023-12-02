using System.Collections.Generic;
using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;
using static GameNetcodeStuff.PlayerControllerB;
using GameNetcodeStuff;
using Unity.Netcode;
using Logger = BepInEx.Logging.Logger;

public struct ScannedEntity
{
    public Collider collider;
    public Vector3 position;
    public float speed;
}

namespace LC_MotionTracker.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class MotionTracker
    {
        private static ManualLogSource logger = null;

        private static List<ScannedEntity> scannedEntities = new();

        private static GameObject MotionTrackerLED;
        private static GameObject SpawnedMotionTracker;

        private static void InitLogger()
        {
            if (logger != null)
            {
                return;
            }

            MotionTrackerLED = LC_API.BundleAPI.BundleLoader.GetLoadedAsset<GameObject>("assets/motion tracker/prefabs/motiontrackerled.prefab");

            logger = new ManualLogSource("LC_MotionTracker_Log");
            Logger.Sources.Add(logger);
        }

        [HarmonyPatch(typeof(PlayerControllerB), "Jump_performed")]
        [HarmonyPostfix]
        public static void PostJump(PlayerControllerB __instance)
        {
            InitLogger();
            logger.LogInfo("Spawning motion tracker");

            SpawnedMotionTracker = UnityEngine.Object.Instantiate(MotionTrackerLED);
            SpawnedMotionTracker.transform.position = __instance.transform.position;
            SpawnedMotionTracker.AddComponent<MotionTrackerScript>();
            SpawnedMotionTracker.transform.localScale = new Vector3(100, 100, 100);
        }

        [HarmonyPatch(typeof(PlayerControllerB), "Update")]
        [HarmonyPostfix]
        public static void UpdateStaminaPostfix(PlayerControllerB __instance)
        {
            return;
            if (!((NetworkBehaviour)__instance).IsOwner || !__instance.isPlayerControlled)
            {
                return;
            }

            InitLogger();
            var lastScannedEntities = new List<ScannedEntity>(scannedEntities);
            scannedEntities.Clear();

            var playerPos = __instance.transform.position;

            var colliders = new Collider[100];
            Physics.OverlapSphereNonAlloc(playerPos, 75, colliders, layerMask: 8 | 524288); // Player | Enemies

            foreach (var collider in colliders)
            {
                var entity = new ScannedEntity { collider = collider, position = collider.transform.position  };

                foreach (var lastEntity in lastScannedEntities)
                {
                    if (lastEntity.collider == collider)
                    {
                        entity.speed = (collider.transform.position - lastEntity.position).magnitude;
                    }
                }

                scannedEntities.Add(entity);
            }

            var i = 0;
            /*(foreach(var entity in scannedEntities)
            {
                logger.LogInfo(entity.speed > 0.06
                    ? $"Moving entity: {i}/{scannedEntities.Count}. Speed: {entity.speed}. {entity.collider.gameObject.name}"
                    : $"Still  entity: {i}/{scannedEntities.Count}. Speed: {entity.speed}. {entity.collider.gameObject}");

                ++i;
            }*/
        }
    }
}