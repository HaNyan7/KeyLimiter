using HarmonyLib;
using KeyLimiter.Features.Hud;
using KeyLimiter.Features.KeyLimit;
using KeyLimiter.Features.Settings;
using System.Reflection;
using UnityModManagerNet;

namespace KeyLimiter;

public static class Main
{
    private static Harmony harmony;

    public static bool Load(UnityModManager.ModEntry modEntry)
    {
        harmony = new Harmony(modEntry.Info.Id);

        ModSettingsFeature.Load(modEntry);

        modEntry.OnToggle = OnToggle;
        modEntry.OnGUI = OnGUI;
        modEntry.OnSaveGUI = OnSaveGUI;

        return true;
    }

    private static bool OnToggle(
        UnityModManager.ModEntry modEntry,
        bool enabled
    )
    {
        if (enabled)
        {
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            KeyLimitRuntime.ResetAll();
            HudFeature.Enable();
        }
        else
        {
            HudFeature.Disable();
            KeyLimitRuntime.ResetAll();
            harmony.UnpatchAll(modEntry.Info.Id);
        }

        return true;
    }

    private static void OnGUI(UnityModManager.ModEntry modEntry)
    {
        ModSettingsFeature.Draw();
    }

    private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
    {
        ModSettingsFeature.Save(modEntry);
    }
}
