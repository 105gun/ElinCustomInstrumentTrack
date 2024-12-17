using System;
using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.Linq;
using System.Reflection;
using System.IO;
using UnityEngine.Assertions;
using System.Reflection.Emit;
using HarmonyLib.Tools;

namespace CustomTrackMod;

public enum PrivateLogLevel
{
    None,
    Error,
    Warning,
    Info,
    Debug
};

[BepInPlugin("105gun.customtrack.mod", "Custom Instrument Track", "1.1.0.0")]
public class Plugin : BaseUnityPlugin
{
    static PrivateLogLevel pluginLogLevel = PrivateLogLevel.Info;
    static string modName = "CustomTrack";

    private void Start()
    {
        ModLog("Initializing");
        var harmony = new Harmony("105gun.customtrack.mod");
        harmony.PatchAll();
        CustomTrackPatch.Init();
        ModLog("Initialization completed");
    }

    public static void ModLog(string message, PrivateLogLevel logLevel = PrivateLogLevel.Info)
    {
        if (logLevel > pluginLogLevel)
        {
            return;
        }
        switch (logLevel)
        {
            case PrivateLogLevel.Error:
                message = $"[{modName}][Error] {message}";
                break;
            case PrivateLogLevel.Warning:
                message = $"[{modName}][Warning] {message}";
                break;
            case PrivateLogLevel.Info:
                message = $"[{modName}][Info] {message}";
                break;
            case PrivateLogLevel.Debug:
                message = $"[{modName}][Debug] {message}";
                break;
            default:
                break;
        }
        System.Console.WriteLine(message);
    }
}