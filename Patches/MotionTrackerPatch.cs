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
    [HarmonyPatch(typeof(WalkieTalkie))]
    internal class MotionTracker
    {
        private static ManualLogSource logger = null;

        private static List<ScannedEntity> scannedEntities = new();

        public static void InitLogger()
        {
            if (logger == null)
            {
                logger = new ManualLogSource("LC_MotionTracker_Log");
                BepInEx.Logging.Logger.Sources.Add(logger);
            }
        }


        [HarmonyPatch(typeof(PlayerControllerB), "Update")]
        [HarmonyPostfix]
        public static void UpdateStaminaPostfix(PlayerControllerB __instance)
        {
            if (((NetworkBehaviour)__instance).IsOwner && __instance.isPlayerControlled)
            {
                InitLogger();
                var lastScannedEntities = new List<ScannedEntity>(scannedEntities);
                scannedEntities.Clear();

                Vector3 playerPos = __instance.transform.position;

                var colliders = Physics.OverlapSphere(playerPos, 100, layerMask: 8 | 524288); // Player | Enemies

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

                int i = 0;
                foreach(var entity in scannedEntities)
                {
                    if (entity.speed > 0.06)
                    {
                        logger.LogInfo($"Moving entity: {i}/{scannedEntities.Count}. Speed: {entity.speed}. {entity.collider.gameObject.name}");
                    }
                    else
                    {
                        logger.LogInfo($"Still  entity: {i}/{scannedEntities.Count}. Speed: {entity.speed}. {entity.collider.gameObject}");
                    }

                    ++i;
                }
            }
        }
    }
}