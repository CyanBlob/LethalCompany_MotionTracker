using System;
using BepInEx.Configuration;
using GameNetcodeStuff;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

public class MotionTrackerConfig
{
    private static int MotionTrackerCostLocal = 30;
    private static float MotionTrackerBatteryDurationLocal = 100f;
    private static float MotionTrackerSpeedDetectLocal = 0.05f;
    private static float MotionTrackerRangeLocal = 50f;

    public static int MotionTrackerCost = 30;
    public static float MotionTrackerBatteryDuration = 100f;
    public static float MotionTrackerSpeedDetect = 0.05f;
    public static float MotionTrackerRange = 50f;

    private static void SetValues(int Cost, float BatteryDuration, float SpeedDetect, float Range)
    {
        MotionTrackerCost = Cost;
        MotionTrackerBatteryDuration = BatteryDuration;
        MotionTrackerSpeedDetect = SpeedDetect;
        MotionTrackerRange = Range;
    }
    private static void SetToLocalValues() => SetValues(MotionTrackerCostLocal, MotionTrackerBatteryDurationLocal, MotionTrackerSpeedDetectLocal, MotionTrackerRangeLocal);

    public static void LoadConfig(ConfigFile config)
    {
        Debug.Log(config);

        MotionTrackerCostLocal = Math.Clamp(config.Bind("General", "MotionTrackerCost", 30, "Motion Tracker's cost").Value, 0, 9999);
        MotionTrackerBatteryDurationLocal = Mathf.Clamp(config.Bind("General", "MotionTrackerBatteryDuration", 100f, "Motion Tracker's battery life").Value, 0f, 9999f);
        MotionTrackerSpeedDetectLocal = Mathf.Clamp(config.Bind("General", "MotionTrackerSpeedDetect", 0.05f, "Minimum speed at which entities can be detected by the Motion Tracker (0.05 is faster than a crouch walk)").Value, 0f, 9999f);
        MotionTrackerRangeLocal = Mathf.Clamp(config.Bind("General", "MotionTrackerRange", 50f, "Motion Tracker's range of action").Value, 0f, 9999f);

        SetToLocalValues();
    }

    public static byte[] GetSettings()
    {
        byte[] data = new byte[17];
        data[0] = 1;
        Array.Copy(BitConverter.GetBytes(MotionTrackerCostLocal), 0, data, 1, 4);
        Array.Copy(BitConverter.GetBytes(MotionTrackerBatteryDurationLocal), 0, data, 5, 4);
        Array.Copy(BitConverter.GetBytes(MotionTrackerSpeedDetectLocal), 0, data, 9, 4);
        Array.Copy(BitConverter.GetBytes(MotionTrackerRangeLocal), 0, data, 13, 4);
        return data;
    }

    public static void SetSettings(byte[] data)
    {
        switch (data[0])
        {
            case 1:
                {
                    MotionTrackerCost = BitConverter.ToInt32(data, 1);
                    MotionTrackerBatteryDuration = BitConverter.ToSingle(data, 5);
                    MotionTrackerSpeedDetect = BitConverter.ToSingle(data, 9);
                    MotionTrackerRange = BitConverter.ToSingle(data, 13);
                    break;
                }
            default:
                {
                    throw new Exception("Invalid version byte");
                }
        }
    }

    // networking

    private static bool IsHost() => NetworkManager.Singleton.IsHost;

    public static void OnRequestSync(ulong clientID, FastBufferReader reader)
    {
        if (!IsHost()) return;

        Debug.Log("MotionTrackerLog: Sending config to client " + clientID);
        byte[] data = GetSettings();
        FastBufferWriter dataOut = new(data.Length, Unity.Collections.Allocator.Temp, data.Length);
        try
        {
            dataOut.WriteBytes(data);
            NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage("MotionTracker_OnReceiveConfigSync", clientID, dataOut, NetworkDelivery.Reliable);
        }
        catch (Exception e)
        {
            Debug.LogError("MotionTrackerLog: Failed to send config: " + e);
        }
        finally
        {
            dataOut.Dispose();
        }
    }

    public static void OnReceiveSync(ulong clientID, FastBufferReader reader)
    {
        Debug.Log("MotionTrackerLog: Received config from host");
        byte[] data = new byte[16];
        try
        {
            reader.ReadBytes(ref data, 16);
            SetSettings(data);
        }
        catch (Exception e)
        {
            Debug.LogError("MotionTrackerLog: Failed to receive config: " + e);
            SetToLocalValues();
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerControllerB), "ConnectClientToPlayerObject")]
    static void ServerConnect()
    {
        if (IsHost())
        {
            Debug.Log("MotionTrackerLog: Started hosting, using local settings");
            SetToLocalValues();
            NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("MotionTracker_OnRequestConfigSync", OnRequestSync);
        }
        else
        {
            Debug.Log("MotionTrackerLog: Connected to server, requesting settings");
            NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("MotionTracker_OnReceiveConfigSync", OnReceiveSync);
            FastBufferWriter blankOut = new();
            NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage("MotionTracker_OnRequestConfigSync", 0, blankOut, NetworkDelivery.Reliable);
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameNetworkManager), "StartDisconnect")]
    static void ServerDisconnect()
    {
        Debug.Log("MotionTrackerLog: Server disconnect");
        SetToLocalValues();
    }
}
