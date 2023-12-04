using System.Collections.Generic;
using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;
using static GameNetcodeStuff.PlayerControllerB;
using GameNetcodeStuff;
using Unity.Netcode;
using Logger = BepInEx.Logging.Logger;
using LethalLib.Modules;

public struct ScannedEntity
{
    public Collider collider;
    public Vector3 position;
    public float speed;
}

namespace MotionTracker.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class MotionTracker
    {
        private static ManualLogSource logger = null;

        private static List<ScannedEntity> scannedEntities = new();

        private static Item motionTrackerLED;
        private static MotionTrackerScript spawnedMotionTracker;

        private static LineDrawer _lineDrawer;

        private static void InitLogger()
        {
            if (logger != null)
            {
                return;
            }

            _lineDrawer = new LineDrawer();

            /*motionTrackerLED =
                LC_API.BundleAPI.BundleLoader.GetLoadedAsset<MotionTrackerScript>(
                    "assets/motion tracker/prefabs/motiontrackerled.prefab");*/

            logger = new ManualLogSource("LC_MotionTracker_Log");
            Logger.Sources.Add(logger);
        }

        [HarmonyPatch(typeof(PlayerControllerB), "Jump_performed")]
        [HarmonyPostfix]
        public static void PostJump(PlayerControllerB __instance)
        {
            return;
            InitLogger();
            logger.LogInfo("Spawning motion tracker");

            AssetBundle assetBundle = AssetBundle.LoadFromFile("motiontrackerled");

            logger.LogInfo(assetBundle);

            foreach (string name in assetBundle.GetAllAssetNames())
            {
                logger.LogInfo(name);
            }

            motionTrackerLED = assetBundle.LoadAsset<Item>("motiontrackerled");

            spawnedMotionTracker = motionTrackerLED.spawnPrefab.AddComponent<MotionTrackerScript>();

            spawnedMotionTracker.itemProperties = motionTrackerLED;

            Items.RegisterShopItem(motionTrackerLED, 0);

            //spawnedMotionTracker = Object.Instantiate(motionTrackerLED);
            //spawnedMotionTracker.transform.position = __instance.transform.position + new Vector3(0, 2f, 0);
            //spawnedMotionTracker.AddComponent<MotionTrackerScript>();
            //spawnedMotionTracker.transform.localScale = new Vector3(10, 10, 10);
            //SpawnedMotionTracker.transform.parent = __instance.transform;
        }

        [HarmonyPatch(typeof(PlayerControllerB), "Update")]
        [HarmonyPostfix]
        public static void Update(PlayerControllerB __instance)
        {
            if (!((NetworkBehaviour)__instance).IsOwner || !__instance.isPlayerControlled)
            {
                return;
            }

            if (spawnedMotionTracker != null)
            {
                InitLogger();
                _lineDrawer.DrawLineInGameView(__instance.transform.position, spawnedMotionTracker.transform.position,
                    new Color(0, 1, 1));
            }

            return;
            /*if (!((NetworkBehaviour)__instance).IsOwner || !__instance.isPlayerControlled)
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
                var entity = new ScannedEntity { obj = collider, position = collider.transform.position };

                foreach (var lastEntity in lastScannedEntities)
                {
                    if (lastEntity.obj == collider)
                    {
                        entity.speed = (collider.transform.position - lastEntity.position).magnitude;
                    }
                }

                scannedEntities.Add(entity);
            }

            var i = 0;*/
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