using KeyLimiter.Features.Settings;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

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

    internal static int RemainingKeyCount { get; private set; }

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
            ModSettingsFeature.Settings.LimitCount
        );

        if (state.Limit != limit)
        {
            state.Limit = limit;
            state.RegisteredKeys.Clear();
        }

        RemainingKeyCount = Mathf.Max(
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
            RemainingKeyCount = Mathf.Max(
                0,
                state.Limit - state.RegisteredKeys.Count
            );
            return true;
        }

        if (ModSettingsFeature.Settings.ExeededAction
            == KeyLimitExceededAction.Die)
        {
            player.Die(
                false,
                false,
                RDString.language == SystemLanguage.Korean
                    ? "키 제한을 초과했습니다."
                    : "Key limit exceeded.",
                false
            );
        }

        return false;
    }

    internal static void Reset(scrPlayer player)
    {
        if (player != null)
            playerStates.Remove(player);

        RemainingKeyCount = Mathf.Max(
            0,
            ModSettingsFeature.Settings.LimitCount
        );
    }

    internal static void ResetAll()
    {
        playerStates = new ConditionalWeakTable<scrPlayer, PlayerState>();
        RemainingKeyCount = Mathf.Max(
            0,
            ModSettingsFeature.Settings.LimitCount
        );
    }

    private static PlayerState CreatePlayerState(scrPlayer _)
    {
        return new PlayerState();
    }
}
