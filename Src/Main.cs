using HarmonyLib;
using KeyLimiter.Display;
using KeyLimiter.KeyLimit;
using KeyLimiter.Settings;
using System.Reflection;
using UnityModManagerNet;

namespace KeyLimiter;

// TODO: 코드 최적화
public static class Main
{
    private static Harmony harmony;

    public static bool Load(UnityModManager.ModEntry modEntry)
    {
        harmony = new Harmony(modEntry.Info.Id);

        ModSettingsController.Load(modEntry);

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
            DisplayController.Create();
        }
        else
        {
            DisplayController.Destroy();
            KeyLimitRuntime.ResetAll();
            harmony.UnpatchAll(modEntry.Info.Id);
        }

        return true;
    }

    private static void OnGUI(UnityModManager.ModEntry modEntry)
    {
        if (!ModSettingsController.Draw())
            return;

        KeyLimitRuntime.ResetAll();
    }

    private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
    {
        ModSettingsController.Save(modEntry);
    }
}
