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


[HarmonyPatch]
public static class CustomTrackPatch
{
    /*
    static void DumpBGM(BGMData data)
    {
        Plugin.ModLog($"{data.name}: Length {data.clip.length} parts.count {data.song.parts.Count}", PrivateLogLevel.Error);
 
        foreach (var field in data.GetType().GetFields())
        {
            Plugin.ModLog($"Field: {field.Name} {field.GetValue(data)}", PrivateLogLevel.Error);
        }

        foreach (var field in data.song.GetType().GetFields())
        {
            Plugin.ModLog($"Field: {field.Name} {field.GetValue(data.song)}", PrivateLogLevel.Error);
        }

        foreach(var part in data.song.parts)
        {
            Plugin.ModLog($"Part: {part.start} {part.duration}", PrivateLogLevel.Error);
        }
    }
*/
    public static bool forceMaxLevel = false;
    public static Dictionary<string, string> instrumentMap = new Dictionary<string, string>();

    public static void Init()
    {
        JsonManager.LoadPrivateData();
    }

    public static string ReplaceIdSong(string idInstrument)
    {
        Plugin.ModLog($"ReplaceIdSong idInstrument = {idInstrument}", PrivateLogLevel.Info);
        
        if (instrumentMap.ContainsKey(idInstrument))
        {
            if (AudioManager.isLegal(instrumentMap[idInstrument]))
            {
                Plugin.ModLog($"\tInst: {idInstrument} => {instrumentMap[idInstrument]}", PrivateLogLevel.Info);
                if (forceMaxLevel)
                {
                    Plugin.ModLog($"ForceMaxLevel: {instrumentMap[idInstrument]}", PrivateLogLevel.Info);
			        EClass.player.knownSongs[instrumentMap[idInstrument]] = new KnownSong();
                    EClass.player.knownSongs[instrumentMap[idInstrument]].lv = 65536;
                }
                return instrumentMap[idInstrument];
            }
            Plugin.ModLog("CheckLegal failed! Please check if your Custom Whatever Loader was enabled!! ", PrivateLogLevel.Error);
            return "";
        }
        Plugin.ModLog($"Couldn't find custom song for this instrument {idInstrument}, using default value. ", PrivateLogLevel.Info);
        return "";
    }

    /* 
     * Insert this ReplaceIdSong into the Run method of AI_PlayMusic
     * If the idSong is empty, using default value. Otherwise, using the custom song.
     */
    [HarmonyPatch(typeof(AI_PlayMusic), nameof(AI_PlayMusic.Run), MethodType.Enumerator)]
    internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        Type Field_DisplayClass15_0 = AccessTools.FirstInner(typeof(AI_PlayMusic), t => t.Name.Contains("DisplayClass"));
        Type Field_d__15 = AccessTools.FirstInner(typeof(AI_PlayMusic), t => t.Name.Contains("__15"));

        if (Field_DisplayClass15_0 == null || Field_d__15 == null)
        {
            Plugin.ModLog("FATAL!!!! Transpiler can't find target Type!!!!!\n\tMod version does not match the game.", PrivateLogLevel.Error);
            return instructions;
        }

        var instructionList = new CodeMatcher(instructions, generator)
            .End()
            .MatchEndBackwards(
                new CodeMatch(o => o.opcode == OpCodes.Stfld &&
                                o.operand.ToString().Contains("idSong")))
            .CreateLabelWithOffsets(1, out Label jmp)
            .Start()
            .MatchStartForward(
                new CodeMatch(OpCodes.Ldloc_3),
                new CodeMatch(o => o.opcode == OpCodes.Call &&
                                o.operand.ToString().Contains("ComputeStringHash")))
            .InsertAndAdvance(
                new CodeInstruction(OpCodes.Ldloc_3),
                Transpilers.EmitDelegate<Func<string, string>>(ReplaceIdSong),
                new CodeInstruction(OpCodes.Stloc_2),
                new CodeInstruction(OpCodes.Ldarg_0), // ldarg.0 NULL
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(Field_d__15, "<>8__1")), // ldfld AI_PlayMusic+<>c__DisplayClass15_0 AI_PlayMusic+<Run>d__15::<>8__1
                new CodeInstruction(OpCodes.Ldloc_2),
                new CodeInstruction(OpCodes.Stfld, AccessTools.Field(Field_DisplayClass15_0, "idSong")), // stfld string AI_PlayMusic+<>c__DisplayClass15_0::idSong
                new CodeInstruction(OpCodes.Ldloc_2),
                new CodeInstruction(OpCodes.Ldstr, ""),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(string), "op_Equality")),
                new CodeInstruction(OpCodes.Brfalse, jmp))
            .InstructionEnumeration();

        /*
        foreach (CodeInstruction instruction in instructionList)
        {
            Plugin.ModLog(instruction.ToString(), PrivateLogLevel.Error);
        }
        */
        return instructionList;
    }
}