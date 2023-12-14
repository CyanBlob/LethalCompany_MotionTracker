using BepInEx;
using LethalLib.Modules;
using UnityEngine;
using UnityEngine.SceneManagement;
using MotionTracker.Patches;
using Unity.Netcode;
using BepInEx.Configuration;

namespace MotionTracker
{
    [BepInPlugin("com.cyanblob.motiontracker", "motiontracker", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        private GameObject MotionTrackerLED;
        private static Item motionTrackerLED_Item;
        private static MotionTrackerScript spawnedMotionTracker;
        public static ConfigEntry<float> BatteryDuration;
        public static ConfigEntry<int> MotionTrackerCost;
        public static ConfigEntry<float> MotionTrackerSpeedDetect;
        public static ConfigEntry<float> MotionTrackerRange;
        private void Awake()
        {
            BatteryDuration = Config.Bind<float>("General", "BatteryDuration", 100f, "Motion Tracker's battery life");
            MotionTrackerCost = Config.Bind<int>("General", "MotionTrackerCost", 30, "Motion Tracker's cost");
            MotionTrackerSpeedDetect = Config.Bind<float>("General", "MotionTrackerSpeedDetect", 0.05f, "Minimum speed at which entities can be detected by the Motion Tracker (0.05 is faster than a crouch walk)");
            MotionTrackerRange = Config.Bind<float>("General", "MotionTrackerRange", 50f, "Motion Tracker's range of action");

            AssetBundle assetBundle = AssetBundle.LoadFromMemory(MotionTrackerResource.motiontrackerled);

            motionTrackerLED_Item =
                assetBundle.LoadAsset<Item>("assets/MotionTrackerItem.asset");


            var netObj = motionTrackerLED_Item.spawnPrefab.GetComponent<NetworkObject>();

            // This seems to be necessary to fix the error when dropping the item in multiplayer
            netObj.AutoObjectParentSync = false;

            spawnedMotionTracker = motionTrackerLED_Item.spawnPrefab.AddComponent<MotionTrackerScript>();

            spawnedMotionTracker.itemProperties = motionTrackerLED_Item;

            spawnedMotionTracker.isInFactory = true;

            Items.RegisterShopItem(motionTrackerLED_Item, MotionTrackerCost.Value);

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
