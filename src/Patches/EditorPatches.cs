using ADOFAI;
using HarmonyLib;
using KeyLimiter.Events;
using KeyLimiter.Gameplay;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace KeyLimiter.Patches;

[HarmonyPatch]
internal static class ParseLevelEventTypePatch
{
    private static MethodBase TargetMethod()
    {
        return AccessTools
            .Method(typeof(RDUtils), "ParseEnum")
            .MakeGenericMethod(typeof(LevelEventType));
    }

    private static bool Prefix(
        string __0,
        ref LevelEventType __result
    )
    {
        if (__0 != KeyLimitEventRegistry.EventName)
            return true;

        __result = KeyLimitEventRegistry.EventType;
        return false;
    }
}

[HarmonyPatch(typeof(ADOStartup), "SetupLevelEventsInfo")]
internal static class RegisterAfterLevelEventSetupPatch
{
    private static void Postfix()
    {
        KeyLimitEventRegistry.Register();
    }
}

[HarmonyPatch(typeof(scnEditor), "LoadEditorProperties")]
internal static class PrepareKeyLimitEditorPatch
{
    private static void Prefix()
    {
        KeyLimitEventRegistry.Register();
        KeyLimitEventRegistry.EnsureIcon();
    }
}

[HarmonyPatch]
internal static class CustomLevelEventInfoLookupPatch
{
    private static readonly MethodInfo DictionaryGetter =
        AccessTools.PropertyGetter(
            typeof(Dictionary<string, LevelEventInfo>),
            "Item"
        );

    private static readonly MethodInfo ReplacementGetter =
        AccessTools.Method(
            typeof(KeyLimitEventRegistry),
            nameof(KeyLimitEventRegistry.GetEventInfo)
        );

    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(LevelEventButton), "Init");
        yield return AccessTools.Method(
            typeof(scnEditor),
            "RemoveEventAtSelected",
            [typeof(LevelEventType)]
        );
        yield return AccessTools.Method(
            typeof(InspectorPanel),
            "ShowPanel",
            [typeof(LevelEventType), typeof(int)]
        );
        yield return AccessTools.Method(
            typeof(LevelEvent),
            "Decode",
            [
                typeof(Dictionary<string, object>),
                typeof(string),
                typeof(bool)
            ]
        );
    }

    private static IEnumerable<CodeInstruction> Transpiler(
        IEnumerable<CodeInstruction> instructions
    )
    {
        foreach (var instruction in instructions)
        {
            if (instruction.Calls(DictionaryGetter))
            {
                instruction.opcode = OpCodes.Call;
                instruction.operand = ReplacementGetter;
            }

            yield return instruction;
        }
    }
}

[HarmonyPatch(typeof(LevelEvent), "Encode", typeof(bool))]
internal static class EncodeKeyLimitEventNamePatch
{
    private static void Postfix(
        LevelEvent __instance,
        Dictionary<string, object> __result
    )
    {
        if (__instance.eventType == KeyLimitEventRegistry.EventType)
            __result["eventType"] = KeyLimitEventRegistry.EventName;
    }
}

[HarmonyPatch(typeof(RDString), "Get")]
internal static class KeyLimitEventLocalizationPatch
{
    private static readonly string NumericEventKey =
        $"editor.{KeyLimitEventRegistry.TypeId}";

    private static readonly string EventKey =
        $"editor.{KeyLimitEventRegistry.EventName}";

    private static readonly string KeyLimitCountKey =
        $"editor.{KeyLimitEventRegistry.EventName}.{KeyLimitEventRegistry.CountPropertyName}";

    private static readonly string ExceededActionKey =
        $"editor.{KeyLimitEventRegistry.EventName}." +
        KeyLimitEventRegistry.ExceededActionPropertyName;

    private static bool Prefix(string __0, ref string __result)
    {
        if (__0 != KeyLimitCountKey
            && __0 != ExceededActionKey
            && __0 != NumericEventKey
            && __0 != EventKey)
        {
            return true;
        }

        __result = GetLocalizedName(__0);
        return false;
    }

    private static string GetLocalizedName(string key)
    {
        if (RDString.language == SystemLanguage.Korean)
            return key == KeyLimitCountKey
                ? "키 제한 수"
                : key == ExceededActionKey
                    ? "제한 초과 시 동작"
                    : "키 제한";

        return key == KeyLimitCountKey
            ? "Key Limit Count"
            : key == ExceededActionKey
                ? "On Limit Exceeded"
                : "Key Limiter";
    }
}

[HarmonyPatch(
    typeof(RDString),
    nameof(RDString.GetEnumValue),
    typeof(string),
    typeof(string)
)]
internal static class KeyLimitActionLocalizationPatch
{
    private static void Postfix(string __0, string __1, ref string __result)
    {
        if (__0 == null
            || !__0.StartsWith(
                typeof(KeyLimitExceededAction).FullName,
                System.StringComparison.Ordinal
            ))
        {
            return;
        }

        var ignore = __1 == nameof(KeyLimitExceededAction.Ignore);
        __result = RDString.language == SystemLanguage.Korean
            ? ignore ? "입력 무시" : "사망"
            : ignore ? "Ignore Input" : "Die";
    }
}
