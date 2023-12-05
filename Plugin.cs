using BepInEx;
using LethalLib.Modules;
using UnityEngine;
using UnityEngine.SceneManagement;
using MotionTracker.Patches;
using Unity.Netcode;

namespace MotionTracker
{
    [BepInPlugin("com.cyanblob.motiontracker", "motiontracker", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        private GameObject MotionTrackerLED;

        private static Item motionTrackerLED_Item;
        private static MotionTrackerScript spawnedMotionTracker;

        private void Awake()
        {
            AssetBundle assetBundle = AssetBundle.LoadFromMemory(MotionTrackerResource.motiontrackerled);

            motionTrackerLED_Item =
                assetBundle.LoadAsset<Item>("assets/MotionTrackerItem.asset");


            /*motionTrackerLED_Item.spawnPrefab.AddComponent<NetworkObject>();
            var netObj = motionTrackerLED_Item.spawnPrefab.GetComponent<NetworkObject>();

            netObj.name = "MotionNetObj";
            netObj.AutoObjectParentSync = true;
            netObj.SynchronizeTransform = true;
            netObj.SceneMigrationSynchronization = true;
            netObj.SpawnWithObservers = true;*/

            spawnedMotionTracker = motionTrackerLED_Item.spawnPrefab.AddComponent<MotionTrackerScript>();

            spawnedMotionTracker.itemProperties = motionTrackerLED_Item;

            Items.RegisterShopItem(motionTrackerLED_Item, 30);

            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(motionTrackerLED_Item.spawnPrefab);

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (Utils.Main.IsIngame())
            {
                LC_API.ServerAPI.ModdedServer.SetServerModdedOnly();
            }
        }
    }
}