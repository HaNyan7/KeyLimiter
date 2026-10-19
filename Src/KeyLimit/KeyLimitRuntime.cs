using KeyLimiter.Localization;
using KeyLimiter.Settings;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Strings = KeyLimiter.Localization.strings;

namespace KeyLimiter.KeyLimit;

internal static class KeyLimitRuntime
{
    private sealed class PlayerState
    {
        internal readonly Dictionary<object, int> PreviousKeyFrequencies = [];
    }

    private static ConditionalWeakTable<scrPlayer, PlayerState> playerStates =
        new();

    internal static void PrepareInput(scrPlayer player)
    {
        if (!ModSettingsController.Settings.Enable
            || player == null
            || !player.alive)
        {
            return;
        }

        var state = playerStates.GetValue(
            player,
            CreatePlayerState
        );

        KeyState.ClearKey();

        state.PreviousKeyFrequencies.Clear();

        foreach (var pair in player.keyFrequency)
        {
            state.PreviousKeyFrequencies[pair.Key] = pair.Value;
        }
    }

    internal static bool CompleteInput(scrPlayer player)
    {
        if (!ModSettingsController.Settings.Enable)
        {
            Reset(player);
            return true;
        }

        if (player == null || !player.alive)
            return true;

        var state = playerStates.GetValue(
            player,
            CreatePlayerState
        );

        if (KeyState.Limit <= 0)
            return true;

        foreach (var pair in player.keyFrequency)
        {
            state.PreviousKeyFrequencies.TryGetValue(
                pair.Key,
                out var previousCount
            );

            if (pair.Value > previousCount)
                KeyState.RegisterKey(pair.Key);
        }

        if (KeyState.CanCommit())
        {
            KeyState.Commit();
            return true;
        }

        if (ModSettingsController.Settings.ExceededAction
            == KeyLimitExceededAction.Die)
        {
            player.Die(
                false,
                false,
                Localizer.Get(nameof(Strings.ModGameOver)),
                false
            );
        }

        return false;
    }

    internal static void Reset(scrPlayer player)
    {
        if (player != null)
            playerStates.Remove(player);

        KeyState.Reset(
            ModSettingsController.Settings.AvailableKeys
        );
    }

    internal static void ResetAll()
    {
        playerStates = new ConditionalWeakTable<scrPlayer, PlayerState>();
        KeyState.Reset(
            ModSettingsController.Settings.AvailableKeys
        );
    }

    private static PlayerState CreatePlayerState(scrPlayer _)
    {
        return new PlayerState();
    }
}
