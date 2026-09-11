using HarmonyLib;
using KeyLimiter.Gameplay;
using KeyLimiter.UI;

namespace KeyLimiter.Patches;

[HarmonyPatch(typeof(scrPlayer), nameof(scrPlayer.CountValidKeysPressed))]
internal static class EnforceKeyLimitPatch
{
    private static void Prefix(scrPlayer __instance)
    {
        KeyLimitRuntime.PrepareInput(__instance);
    }

    private static void Postfix(scrPlayer __instance, ref int __result)
    {
        if (!KeyLimitRuntime.CompleteInput(__instance))
            __result = 0;
    }
}

[HarmonyPatch(typeof(scrController), "PlayerControl_Update")]
internal static class UpdateKeyLimitPopupPatch
{
    private static void Postfix(scrController __instance)
    {
        KeyLimitPopup.Update(__instance);
    }
}

[HarmonyPatch(typeof(scrPlayer), nameof(scrPlayer.Init))]
internal static class ResetKeyLimitOnInitPatch
{
    private static void Postfix(scrPlayer __instance)
    {
        KeyLimitRuntime.Reset(__instance);
    }
}

[HarmonyPatch(typeof(scrPlayer), nameof(scrPlayer.Rewind))]
internal static class ResetKeyLimitOnRewindPatch
{
    private static void Postfix(scrPlayer __instance)
    {
        KeyLimitRuntime.Reset(__instance);
    }
}
