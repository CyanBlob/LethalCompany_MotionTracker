using BepInEx;
using HarmonyLib;
using LC_API;
using LethalLib.Modules;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.SceneManagement;
using BepInEx;
using BepInEx.Logging;
using LethalLib.Modules;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using MotionTracker.Patches;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

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
            // Plugin startup logic
            AssetBundle assetBundle = AssetBundle.LoadFromMemory(MotionTrackerResource.motiontrackerled);

            Logger.LogInfo(assetBundle);

            foreach (string name in assetBundle.GetAllAssetNames())
            {
                Logger.LogInfo($"Asset name: {name}");
            }

            motionTrackerLED_Item =
                assetBundle.LoadAsset<Item>("assets/MotionTrackerItem.asset");


            motionTrackerLED_Item.spawnPrefab.AddComponent<NetworkObject>();
            var netObj = motionTrackerLED_Item.spawnPrefab.GetComponent<NetworkObject>();

            netObj.name = "MotionNetObj";


            spawnedMotionTracker = motionTrackerLED_Item.spawnPrefab.AddComponent<MotionTrackerScript>();

            spawnedMotionTracker.itemProperties = motionTrackerLED_Item;

            Logger.LogInfo($"Registering: {motionTrackerLED_Item}");
            Items.RegisterShopItem(motionTrackerLED_Item, 30);
            Logger.LogInfo($"Done registering: {motionTrackerLED_Item}");

            foreach (var item in Items.shopItems)
            {
                Logger.LogInfo($"Item: {item.modName}");
            }

            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(motionTrackerLED_Item.spawnPrefab);

            //SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (Utils.Main.IsIngame())
            {
                //LC_API.ServerAPI.ModdedServer.SetServerModdedOnly();
            }
        }
    }
}
