using KeyLimiter.Features.Localization;
using KeyLimiter.Features.Settings;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Strings = KeyLimiter.Localization.strings;

namespace KeyLimiter.Features.KeyLimit;

internal static class KeyLimitRuntime
{
    private sealed class PlayerState
    {
        internal readonly Dictionary<object, int> PreviousKeyFrequencies = [];
        internal readonly HashSet<object> RegisteredKeys = [];
        internal readonly HashSet<object> NewKeys = [];

        internal int Limit = int.MinValue;
    }

    private static ConditionalWeakTable<scrPlayer, PlayerState> playerStates =
        new();

    internal static int AvailableKeys { get; private set; }

    internal static void PrepareInput(scrPlayer player)
    {
        if (!ModSettingsFeature.Settings.Enable
            || player == null
            || !player.alive)
        {
            return;
        }

        var state = playerStates.GetValue(
            player,
            CreatePlayerState
        );

        var limit = Mathf.Max(
            0,
            ModSettingsFeature.Settings.AvailableKeys
        );

        if (state.Limit != limit)
        {
            state.Limit = limit;
            state.RegisteredKeys.Clear();
        }

        AvailableKeys = Mathf.Max(
            0,
            state.Limit - state.RegisteredKeys.Count
        );

        state.PreviousKeyFrequencies.Clear();

        foreach (var pair in player.keyFrequency)
        {
            state.PreviousKeyFrequencies[pair.Key] = pair.Value;
        }
    }

    internal static bool CompleteInput(scrPlayer player)
    {
        if (!ModSettingsFeature.Settings.Enable)
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

        if (state.Limit <= 0)
            return true;

        state.NewKeys.Clear();

        foreach (var pair in player.keyFrequency)
        {
            state.PreviousKeyFrequencies.TryGetValue(
                pair.Key,
                out var previousCount
            );

            if (pair.Value > previousCount
                && !state.RegisteredKeys.Contains(pair.Key))
            {
                state.NewKeys.Add(pair.Key);
            }
        }

        if (state.RegisteredKeys.Count + state.NewKeys.Count <= state.Limit)
        {
            state.RegisteredKeys.UnionWith(state.NewKeys);
            AvailableKeys = Mathf.Max(
                0,
                state.Limit - state.RegisteredKeys.Count
            );
            return true;
        }

        if (ModSettingsFeature.Settings.ExceededAction
            == KeyLimitExceededAction.Die)
        {
            player.Die(
                false,
                false,
                LocalizationFeature.Get(nameof(Strings.ModGameOver)),
                false
            );
        }

        return false;
    }

    internal static void Reset(scrPlayer player)
    {
        if (player != null)
            playerStates.Remove(player);

        AvailableKeys = Mathf.Max(
            0,
            ModSettingsFeature.Settings.AvailableKeys
        );
    }

    internal static void ResetAll()
    {
        playerStates = new ConditionalWeakTable<scrPlayer, PlayerState>();
        AvailableKeys = Mathf.Max(
            0,
            ModSettingsFeature.Settings.AvailableKeys
        );
    }

    private static PlayerState CreatePlayerState(scrPlayer _)
    {
        return new PlayerState();
    }
}
