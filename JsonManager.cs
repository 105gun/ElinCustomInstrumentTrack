using System;
using System.IO;
using Cwl.API;
using Newtonsoft.Json;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

namespace CustomTrackMod;

class JsonManager
{
    static string currentPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

    public static void SaveBGMJsonData(BGMData bgmData)
    {
        string path = Path.Combine(currentPath, "Sound", $"{bgmData.name}.json");
        bgmData.WriteMetaTo(path);
    }

    public static void UpdateFromJson(string path, SerializablePrivateData data)
    {
        Plugin.ModLog($"Loading from {path}, {data.instrumentMap.Count} new items", PrivateLogLevel.Info);
        foreach (var kvp in data.instrumentMap)
        {
            if (CustomTrackPatch.instrumentMap.ContainsKey(kvp.Key))
            {
                Plugin.ModLog($"\tKey {kvp.Key} already exists. Overwriting: {CustomTrackPatch.instrumentMap[kvp.Key]} => {kvp.Value}", PrivateLogLevel.Warning);
            }

            CustomTrackPatch.instrumentMap[kvp.Key] = kvp.Value;
            if (data.forceMaxLevel)
            {
                CustomTrackPatch.maxLevelSet.Add(kvp.Value);
            }
        }
    }

    public static void LoadPrivateData()
    {
        string path = Path.Combine(currentPath, "CIT.json");
        if (ReadConfig(path, out SerializablePrivateData? data) && data != null)
        {
            UpdateFromJson(path, data);
        }
        else
        {
            // New file
            CustomTrackPatch.instrumentMap = new();
            CustomTrackPatch.instrumentMap["lute"] = "HouraiDensetsu";
            SavePrivateData();
        }
    }

    public static void SavePrivateData()
    {
        var data = new SerializablePrivateData
        {
            forceMaxLevel = false,
            instrumentMap = CustomTrackPatch.instrumentMap
        };
        string path = Path.Combine(currentPath, "CIT.json");
        WriteConfig(data, path);
    }

    public static void LoadOtherData()
    {
        foreach (var directoryInfo in Cwl.Helper.FileUtil.PackageIterator.GetLoadedPackages())
        {
            string path = Path.Combine(directoryInfo.FullName, "CIT.json");

            if (directoryInfo.FullName == currentPath)
            {
                continue;
            }

            if (ReadConfig(path, out SerializablePrivateData? data) && data != null)
            {
                UpdateFromJson(path, data);
            }
        }
    }

    // Porting from https://github.com/gottyduke/Elin.Plugins/blob/master/CustomWhateverLoader/Helper/ConfigCereal.cs
    internal static void WriteConfig<T>(T data, string path)
    {
        try {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);

            using var sw = new StreamWriter(path);
            sw.Write(JsonConvert.SerializeObject(data, Formatting.Indented));
        } catch (Exception ex) {
            Plugin.ModLog($"internal failure: {ex.Message}", PrivateLogLevel.Error);
            // noexcept
        }
    }

    internal static bool ReadConfig<T>(string path, out T? inferred)
    {
        try {
            if (File.Exists(path)) {
                using var sr = new StreamReader(path);
                inferred = JsonConvert.DeserializeObject<T>(sr.ReadToEnd());
                return true;
            }
        } catch (Exception ex) {
            Plugin.ModLog($"failed to read config: {ex.Message}", PrivateLogLevel.Error);
            throw;
        }

        inferred = default;
        return false;
    }
}