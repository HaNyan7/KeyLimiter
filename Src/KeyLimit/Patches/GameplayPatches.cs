using HarmonyLib;
using KeyLimiter.Display;

namespace KeyLimiter.KeyLimit.Patches;

[HarmonyPatch(
    typeof(scrPlayer),
    nameof(scrPlayer.CountValidKeysPressed)
)]
internal static class EnforceKeyLimitPatch
{
    private static void Prefix(scrPlayer __instance)
    {
        KeyLimitRuntime.PrefixInput(__instance);
    }

    private static void Postfix(
        scrPlayer __instance,
        ref int __result
    )
    {
        if (!KeyLimitRuntime.PostfixInput(__instance))
            __result = 0;
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

[HarmonyPatch(typeof(scnGame), "OnDestroy")]
internal static class ResetKeyLimitOnGameEndPatch
{
    private static void Prefix()
    {
        DisplayController.Stop();
        KeyLimitRuntime.ResetAll();
    }
}

[HarmonyPatch(typeof(scnGame), nameof(scnGame.Play))]
internal static class ShowDisplayOnPlayPatch
{
    private static void Postfix(bool __result)
    {
        if (__result)
            DisplayController.Play();
    }
}

[HarmonyPatch(typeof(scnEditor), nameof(scnEditor.SwitchToEditMode))]
internal static class HideDisplayOnEditPatch
{
    private static void Prefix()
    {
        DisplayController.Stop();
    }
}
