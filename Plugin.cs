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

namespace CardboardBoxMod;

public enum PrivateLogLevel
{
    None,
    Error,
    Warning,
    Info,
    Debug
};

[BepInPlugin("105gun.replacetrack.mod", "Tactical Cardboard Box", "1.0.0.0")]
public class Plugin : BaseUnityPlugin
{
    static PrivateLogLevel pluginLogLevel = PrivateLogLevel.Debug;
    static string modName = "ReplaceTrack";

    private void Start()
    {
        ModLog("Initializing");
        var harmony = new Harmony("105gun.replacetrack.mod");
        harmony.PatchAll();
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


[HarmonyPatch]
public static class ReplaceTrackPatch
{
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

    static string idSong = "";
    public static void DUMMY()
    {
        idSong = "";
        if ((idSong = ReplaceIdSong("dummy")) == "")
        {
            Plugin.ModLog("ORIGIN CODE", PrivateLogLevel.Error);
            idSong = "ORIGIN";
        }
    }

    public static string ReplaceIdSong(string id)
    {
        Plugin.ModLog($"ReplaceIdSong {id}", PrivateLogLevel.Error);
        BGMData violin_chaconne = EClass.Sound.GetData($"Instrument/violin_chaconne") as BGMData;
        DumpBGM(violin_chaconne);

        BGMData soundData = EClass.Sound.GetData($"Instrument/custom_NewFantasy") as BGMData;
        DumpBGM(soundData);
            // Dump violin_chaconne.song.parts

       // BGMData soundData = EClass.Sound.GetData($"Instrument/violin_chaconne") as BGMData;
        //AudioLoader.DumpSoundData(soundData);
        // if (EClass.player.knownSongs.ContainsKey("Instrument/custom_NewFantasy"))
		{
			EClass.player.knownSongs["custom_NewFantasy"].lv = 10000;
		}
        // Print all fields
        // soundData.song = violin_chaconne.song;
        soundData.song.parts = new List<BGMData.Part>();
        for (int i = 0 ; i < 16; i++)
        {
            soundData.song.parts.Add(new BGMData.Part
            {
                start = (i + 1) * 3.5f,
                duration = 4
            });
        }
        //soundData.type = violin_chaconne.type;
        //soundData.variations = violin_chaconne.variations;

        return "";
    }

    /* (AI_PlayMusic.Run)
     *
     * Original code:
     *      string id = this.tool.id;
     *      uint num = <PrivateImplementationDetails>.ComputeStringHash(id);
     *      ......
     *      idSong = "violin_chaconne";
     *
     * Original IL code:
     *      IL_024C: ldloc.3
     *      IL_024D: call uint32 <PrivateImplementationDetails>::ComputeStringHash(string)
     *      IL_0252: stloc.s 4
     *      ......
     *      IL_0627: ldarg.0
     *      IL_0628: ldfld AI_PlayMusic+<>c__DisplayClass15_0 AI_PlayMusic+<Run>d__15::<>8__1
     *      IL_062D: ldstr "violin_chaconne"
     *      IL_0632: stfld string AI_PlayMusic+<>c__DisplayClass15_0::idSong
     *
     * Transpiled code:
     *      string id = this.tool.id;
     *      idSong = ReplaceTrackPatch.ReplayIdSong(id);
     *
     * Transpiled IL code:
     *      IL_024C: nop
     *      ......
     *      IL_????: ldarg.0
     *      IL_????: ldfld AI_PlayMusic+<>c__DisplayClass15_0 AI_PlayMusic+<Run>d__15::<>8__1
     *      IL_????: ldloc.3
     *      IL_????: call static string CardboardBoxMod.ReplaceTrackPatch::ReplayIdSong(string id)
     *      IL_????: stfld string AI_PlayMusic+<>c__DisplayClass15_0::idSong
     */

#if false
    public static System.Reflection.MethodBase TargetMethod()
	{
        // Patching AI_PlayMusic.Run
		return AccessTools.EnumeratorMoveNext(AccessTools.Method("AI_PlayMusic:Run", new Type[] {}, null));
	}

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> instructionList = new List<CodeInstruction>(instructions);
        int startIndex = -1;
        int endIndex = -1;

        for (int i = 0; i < instructionList.Count; i++)
        {
            string instructionStr = instructionList[i].ToString();
            if (instructionStr == "call static uint <PrivateImplementationDetails>::ComputeStringHash(string s)")
            {
                startIndex = i;
            }
            if (instructionStr == "stfld string AI_PlayMusic+<>c__DisplayClass15_0::idSong")
            {
                endIndex = i;
            }
        }

        if (startIndex != -1 && endIndex != -1 && startIndex < endIndex)
        {
            for (int i = startIndex; i < endIndex - 3; i++)
            {
                instructionList[i].opcode = OpCodes.Nop;
            }

            instructionList.Remove(instructionList[endIndex - 1]);
            instructionList.Insert(endIndex - 1, new CodeInstruction(instructionList[startIndex - 1]));
            instructionList.Insert(endIndex, CodeInstruction.Call(() => ReplaceIdSong(default)));

            instructionList[startIndex - 1].opcode = OpCodes.Nop;
        }
        return instructionList;
/*

        foreach (CodeInstruction instruction in instructionList)
        {
            Plugin.ModLog(instruction.ToString(), PrivateLogLevel.Error);
        }
        Plugin.ModLog($"HACK END", PrivateLogLevel.Error);*/
    }
#else
    // Codes from FreshCloth. Very smart.
    [HarmonyPatch(typeof(AI_PlayMusic), nameof(AI_PlayMusic.Run), MethodType.Enumerator)]
    internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        /*
        var instructionList = new CodeMatcher(instructions, generator)
            .End()
            .MatchEndBackwards(
                new CodeMatch(o => o.opcode == OpCodes.Stfld &&
                                o.operand.ToString().Contains("idSong")))
            .Insert(
                new CodeInstruction(OpCodes.Pop),
                new CodeInstruction(OpCodes.Ldloc_3),
                Transpilers.EmitDelegate<Func<string, string>>(ReplaceIdSong))
            .MatchStartBackwards(
                new CodeMatch(OpCodes.Ldarg_0))
            .CreateLabel(out Label jmp)
            .Start()
            .MatchStartForward(
                new CodeMatch(OpCodes.Ldloc_3),
                new CodeMatch(o => o.opcode == OpCodes.Call &&
                                o.operand.ToString().Contains("ComputeStringHash")))
            .InsertAndAdvance(
                new CodeInstruction(OpCodes.Br, jmp))
            .InstructionEnumeration();
        foreach (CodeInstruction instruction in instructionList)
        {
            Plugin.ModLog(instruction.ToString(), PrivateLogLevel.Error);
        }
        return instructionList;*/

        Type fieldDisplayClass15_0 = AccessTools.FirstInner(typeof(AI_PlayMusic), t => t.Name.Contains("DisplayClass15_0"));
        Type fieldd__15 = AccessTools.FirstInner(typeof(AI_PlayMusic), t => t.Name.Contains("__15"));

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
                /* ldfld AI_PlayMusic+<>c__DisplayClass15_0 AI_PlayMusic+<Run>d__15::<>8__1 */
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(fieldd__15, "<>8__1")),
                new CodeInstruction(OpCodes.Ldloc_2),
                new CodeInstruction(OpCodes.Stfld, AccessTools.Field(fieldDisplayClass15_0, "idSong")),
                new CodeInstruction(OpCodes.Ldloc_2),
                new CodeInstruction(OpCodes.Ldstr, ""),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(string), "op_Equality")),
                new CodeInstruction(OpCodes.Brfalse, jmp))
            .InstructionEnumeration();
        foreach (CodeInstruction instruction in instructionList)
        {
            Plugin.ModLog(instruction.ToString(), PrivateLogLevel.Error);
        }
        return instructionList;
    }
#endif
}