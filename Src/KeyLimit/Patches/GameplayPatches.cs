using HarmonyLib;
using KeyLimiter.Display;
using KeyLimiter.Settings;

namespace KeyLimiter.KeyLimit.Patches;

[HarmonyPatch(
    typeof(scrPlayer),
    nameof(scrPlayer.CountValidKeysPressed)
)]
internal static class EnforceKeyLimitPatch
{
    private static void Prefix(scrPlayer __instance)
    {
        KeyLimitRuntime.PrepareInput(__instance);
    }

    private static void Postfix(
        scrPlayer __instance,
        ref int __result
    )
    {
        if (!KeyLimitRuntime.CompleteInput(__instance))
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

[HarmonyPatch(typeof(scnGame), "Awake")]
internal static class ShowDisplayOnGameStartPatch
{
    private static void Postfix()
    {
        DisplayController.SetVisible(
            ModSettingsController.Settings.Enable
        );
    }
}

[HarmonyPatch(typeof(scnGame), "OnDestroy")]
internal static class HideDisplayOnGameEndPatch
{
    private static void Prefix()
    {
        DisplayController.SetVisible(false);
        KeyLimitRuntime.ResetAll();
    }
}
