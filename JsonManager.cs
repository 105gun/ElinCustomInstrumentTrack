using System;
using System.IO;
using Cwl.API;
using Newtonsoft.Json;
using UnityEngine;

namespace CustomTrackMod;

class JsonManager
{
    public static void SaveBGMJsonData(BGMData bgmData)
    {
        string path = Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Sound", $"{bgmData.name}.json");
        bgmData.WriteMetaTo(path);
    }

    public static void SavePrivateData()
    {
        var data = new SerializablePrivateData
        {
            forceMaxLevel = CustomTrackPatch.forceMaxLevel,
            instrumentMap = CustomTrackPatch.instrumentMap
        };
        string path = Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "CIT.json");
        WriteConfig(data, path);
    }

    public static void LoadPrivateData()
    {
        string path = Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "CIT.json");
        if (ReadConfig(path, out SerializablePrivateData? data) && data != null)
        {
            CustomTrackPatch.forceMaxLevel = data.forceMaxLevel;
            CustomTrackPatch.instrumentMap = data.instrumentMap;
        }
        else
        {
            // New file
            CustomTrackPatch.instrumentMap = new();
            CustomTrackPatch.instrumentMap["lute"] = "HouraiDensetsu";
            SavePrivateData();
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