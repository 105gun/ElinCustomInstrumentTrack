using System;
using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.Linq;
using UnityEngine.Assertions;
using UnityEngine.Networking;

namespace CustomTrackMod;

/*
 * Theoretically, a piece of BGM for instrumentation is cut into parts, which define its start time and duration.
 *
 * However, in actual gameplay, this schedule cannot be strictly enforced. I have no intention of figuring this part out
 * but experiments have shown that every two neighboring parts can accumulate up to 100ms of error, which is unacceptable
 * for music playing. There is also a significant time difference between handheld instruments and furniture instruments.
 *
 * The idea of this workaround is to record Time.time() and the index of the last part, then update the start time online.
 * This ensures that the music the player hears is not significantly glitchy.
 */

// SoundSource.KeepPlay
[HarmonyPatch(typeof(SoundSource), "Play")]
class MusicGlitchWorkaround
{
    static float lastTime = 0;
    static float lastOffset = 0;
    static int lastIndex = -1;
    static void Prefix(SoundSource __instance, SoundData _data, float mtpVolume, Vector3 pos)
    {
        if (_data.name.Contains("Instrument"))
        {
            Plugin.ModLog($"Play instrument! {_data.name}", PrivateLogLevel.Debug);
            float currentTime = Time.time;
            BGMData bgmData = _data as BGMData;
            if (bgmData == null)
            {
                Plugin.ModLog("BGMData is null!", PrivateLogLevel.Debug);
                return;
            }
            if (bgmData.song.index == 0)
            {
                // Reset
                lastTime = currentTime;
                lastIndex = 0;
                lastOffset = 0;
            }
            else if (lastIndex == bgmData.song.index)
            {
                // Player failed in last part
                lastTime = currentTime;
            }
            else
            {
                // Player finished last part, step to next part
                bgmData.song.parts[bgmData.song.index].start = currentTime - lastTime + lastOffset;
                lastTime = currentTime;
                lastIndex = bgmData.song.index;
                lastOffset = bgmData.song.parts[bgmData.song.index].start;
            }
            // Print
            Plugin.ModLog($"Play {bgmData.name} {bgmData.song.index} {bgmData.song.parts[bgmData.song.index].start} {bgmData.song.parts[bgmData.song.index].duration} {__instance.source.time}", PrivateLogLevel.Debug);
            // Print callstack
            //System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace();
            //Plugin.ModLog(stackTrace.ToString(), PrivateLogLevel.Error);
        }
    }
}