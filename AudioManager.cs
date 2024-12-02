using System;
using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.Linq;
using UnityEngine.Assertions;
using UnityEngine.Networking;

namespace CustomTrackMod;

class AudioManager
{
    public static Dictionary<string, BGMData> modPrivateBGMDataDict = new Dictionary<string, BGMData>();

    public static void UpdateBGMParts(BGMData bgmData)
    {
        if (bgmData == null)
        {
            Plugin.ModLog("UpdateBGMParts: BGMData is null!", PrivateLogLevel.Error);
            return;
        }
        bgmData.song.parts = new List<BGMData.Part>();
        for (int i = 0 ; i < (int)(bgmData.clip.length / 3.5) + 1; i++)
        {
            bgmData.song.parts.Add(new BGMData.Part
            {
                start = i * 3.5f,
                duration = 4
            });
        }
    }

    public static void UpdateBGMData(BGMData bgmData)
    {
        Plugin.ModLog($"UpdateBGMData {bgmData.name}", PrivateLogLevel.Debug);
        modPrivateBGMDataDict[bgmData.name] = bgmData;
        EClass.Sound.dictData[$"{bgmData.name}"] = bgmData;
        JsonManager.SaveBGMJsonData(bgmData);
    }

    public static bool isLegal(string musicName)
    {
        SoundData soundData = null;
        BGMData bgmData = null;
        Plugin.ModLog($"CheckLegal {musicName}", PrivateLogLevel.Debug);
        if (!modPrivateBGMDataDict.ContainsKey($"Instrument/{musicName}"))
        {
            soundData = EClass.Sound.GetData($"Instrument/{musicName}");
            if (soundData == null)
            {
                Plugin.ModLog($"BGMData {musicName} was not loaded by custom whatever loader!", PrivateLogLevel.Error);
                return false;
            }
            else if (!(soundData is BGMData))
            {
                // Which means, this SoundData was generated by custom whatever loader and never fixed by our mod.
                // We will create a brand new BGMData with our parameters.
                bgmData = ScriptableObject.CreateInstance<BGMData>();
                bgmData.name = soundData.name;
                bgmData.clip = soundData.clip;
                bgmData.loop = 0;
                bgmData.allowMultiple = true;
                bgmData.skipIfPlaying = false;
                bgmData.important = true;
                bgmData.alwaysPlay = true;
                bgmData.fadeAtStart = false;
                bgmData.delay = 0;
                bgmData.startAt = 0;
                bgmData.fadeLength = 0;
                bgmData.type = SoundData.Type.BGM;
                bgmData.spatial = 1.0f;
                bgmData.pitch = 1.0f;
                bgmData.randomPitch = 0;
                bgmData.chance = 1;
                bgmData.reverbMix = 1;
                bgmData.minInterval = 0;
                bgmData.variationPlayMethod = SoundData.VariationPlayMethod.Sequence;
                bgmData.noSameSound = false;
                bgmData.volumeAsMTP = false;
                bgmData.variations = null;
                bgmData.extraData = null;
                bgmData.variationIndex = 0;
                bgmData.lastVariation = null;
                bgmData.lastPlayed = 0;
                bgmData.altLastPlayed = 0;

                bgmData.song = new BGMData.SongData();
                bgmData.song.fadeIn = 0.1f;
                bgmData.song.fadeOut = 0.5f;
                bgmData.song.failDuration = 0.12f;
                bgmData.song.failPitch = 2.0f;
                bgmData.song.pitchDuration = 0.5f;
            }
            else
            {
                bgmData = soundData as BGMData;
            }
        }
        else
        {
            bgmData = modPrivateBGMDataDict[$"Instrument/{musicName}"];
        }

        if (bgmData != null)
        {
            // Force setting pitch to 1.0f
            bgmData.pitch = 1.0f;
            // Force update parts
            UpdateBGMParts(bgmData);
            UpdateBGMData(bgmData);
        }
        else
        {
            Plugin.ModLog($"BGMData {musicName} == null!", PrivateLogLevel.Error);
            return false;
        }
        return true;
    }
}