using HarmonyLib;
using KeyLimiter.Display;
using KeyLimiter.KeyLimit;
using KeyLimiter.Settings;
using System.Reflection;
using UnityModManagerNet;

namespace KeyLimiter;

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
            DisplayController.SetVisible(
                ModSettingsController.Settings.Enable
                && scnGame.instance != null
            );
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
        DisplayController.SetVisible(
            ModSettingsController.Settings.Enable
            && scnGame.instance != null
        );
    }

    private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
    {
        ModSettingsController.Save(modEntry);
    }
}
