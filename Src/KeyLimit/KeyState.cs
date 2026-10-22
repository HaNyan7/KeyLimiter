using KeyLimiter.Settings;
using System;
using System.Collections.Generic;

namespace KeyLimiter.KeyLimit;

internal static class KeyState
{
    private static readonly HashSet<object> pressedKeys = [];
    private static readonly HashSet<object> usedKeys = [];

    internal static IReadOnlyCollection<object> PressedKeys =>
        pressedKeys;
    internal static IReadOnlyCollection<object> UsedKeys => usedKeys;

    internal static int Limit = Math.Max(
        0,
        ModSettingsController.Settings.AvailableKeys
    );

    internal static int AvailableKeys => Math.Max(
        0,
        Limit - usedKeys.Count
    );

    internal static void ClearKey()
    {
        pressedKeys.Clear();
    }

    internal static void RegisterKey(object key)
    {
        pressedKeys.Add(key);
    }

    internal static bool CanCommit()
    {
        var newKeyCount = 0;

        foreach (var key in pressedKeys)
        {
            if (!usedKeys.Contains(key))
                newKeyCount++;
        }

        return usedKeys.Count + newKeyCount <= Limit;
    }

    internal static void Commit()
    {
        usedKeys.UnionWith(PressedKeys);
    }

    internal static void Reset(int limit)
    {
        Limit = Math.Max(0, limit);
        pressedKeys.Clear();
        usedKeys.Clear();
    }
}
