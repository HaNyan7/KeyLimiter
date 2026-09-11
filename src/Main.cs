using HarmonyLib;
using KeyLimiter.Events;
using KeyLimiter.Gameplay;
using KeyLimiter.UI;
using System.Reflection;
using UnityModManagerNet;

namespace KeyLimiter;

public static class Main
{
    private static Harmony harmony;

    public static bool Load(UnityModManager.ModEntry modEntry)
    {
        harmony = new Harmony(modEntry.Info.Id);
        KeyLimitEventRegistry.SetModDirectory(modEntry.Path);
        KeyLimitPopup.SetModDirectory(modEntry.Path);
        modEntry.OnToggle = OnToggle;

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
            KeyLimitEventRegistry.Register();
        }
        else
        {
            KeyLimitPopup.Reset();
            KeyLimitRuntime.ResetAll();
            KeyLimitEventRegistry.Unregister();
            harmony.UnpatchAll(modEntry.Info.Id);
        }

        return true;
    }
}
