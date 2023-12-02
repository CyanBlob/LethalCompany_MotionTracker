using BepInEx;
using HarmonyLib;
using LC_MotionTracker.Patches;
using LC_API;
using UnityEngine;

namespace LC_MotionTracker
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private readonly Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);
        private GameObject MotionTrackerLED;

        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            harmony.PatchAll(typeof(MotionTracker));

            LC_API.BundleAPI.BundleLoader.OnLoadedBundles += () => {
                Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID}->OnLoadedAssets is called!");
                MotionTrackerLED = LC_API.BundleAPI.BundleLoader.GetLoadedAsset<GameObject>("assets/motion tracker/prefabs/motiontrackerled.prefab");
                Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID}->MotionTracker: {MotionTrackerLED}");
            };
        }
    }
}