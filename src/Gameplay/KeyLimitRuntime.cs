using ADOFAI;
using KeyLimiter.Events;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace KeyLimiter.Gameplay;

internal static class KeyLimitRuntime
{
    private sealed class PlayerState
    {
        internal readonly Dictionary<object, int> ObservedKeyFrequencies = [];
        internal readonly HashSet<object> NewKeysThisInput = [];
        internal readonly HashSet<object> RegisteredKeys = [];
        internal LevelEvent SourceEvent;
        internal KeyLimitExceededAction ExceededAction;
        internal int FloorIndex = int.MinValue;
        internal int Limit;
        internal bool PopupPending;
    }

    private static ConditionalWeakTable<scrPlayer, PlayerState> playerStates =
        new();

    internal static void PrepareInput(scrPlayer player)
    {
        TryGetActiveState(player, out _);
    }

    internal static bool CompleteInput(scrPlayer player)
    {
        if (!TryGetActiveState(player, out var state) || state.Limit == 0)
            return true;

        state.NewKeysThisInput.Clear();
        foreach (var pair in player.keyFrequency)
        {
            state.ObservedKeyFrequencies.TryGetValue(pair.Key, out var previousCount);
            state.ObservedKeyFrequencies[pair.Key] = pair.Value;

            if (pair.Value > previousCount
                && !state.RegisteredKeys.Contains(pair.Key))
            {
                state.NewKeysThisInput.Add(pair.Key);
            }
        }

        if (state.RegisteredKeys.Count + state.NewKeysThisInput.Count <= state.Limit)
        {
            state.RegisteredKeys.UnionWith(state.NewKeysThisInput);
            return true;
        }

        if (state.ExceededAction == KeyLimitExceededAction.Die)
            player.Die(false, false, GetFailMessage(), false);

        return false;
    }

    internal static bool TryTakePopup(scrPlayer player, out int limit)
    {
        limit = 0;
        if (!TryGetActiveState(player, out var state) || !state.PopupPending)
            return false;

        state.PopupPending = false;
        limit = state.Limit;
        return limit > 0;
    }

    internal static void Reset(scrPlayer player)
    {
        if (player != null)
            playerStates.Remove(player);
    }

    internal static void ResetAll()
    {
        playerStates = new ConditionalWeakTable<scrPlayer, PlayerState>();
    }

    private static PlayerState CreatePlayerState(scrPlayer _)
    {
        return new PlayerState();
    }

    private static bool TryGetActiveState(scrPlayer player, out PlayerState state)
    {
        state = null;
        if (player == null || !player.alive || player.currFloor == null)
            return false;

        state = playerStates.GetValue(player, CreatePlayerState);
        UpdateLimit(player, state, player.currFloor.seqID);
        return true;
    }

    private static void UpdateLimit(
        scrPlayer player,
        PlayerState state,
        int floorIndex
    )
    {
        if (state.FloorIndex == floorIndex)
            return;

        var sourceEvent = FindLatestEvent(floorIndex);
        var limit = sourceEvent == null
            ? 0
            : Math.Max(0, sourceEvent.GetInt(KeyLimitEventRegistry.CountPropertyName));
        var exceededAction = GetExceededAction(sourceEvent);

        var sourceChanged = !ReferenceEquals(sourceEvent, state.SourceEvent);
        if (floorIndex < state.FloorIndex
            || sourceChanged
            || limit != state.Limit
            || exceededAction != state.ExceededAction)
        {
            state.RegisteredKeys.Clear();
            SnapshotFrequencies(player, state);
            state.PopupPending = sourceEvent != null && limit > 0;
        }

        state.SourceEvent = sourceEvent;
        state.ExceededAction = exceededAction;
        state.FloorIndex = floorIndex;
        state.Limit = limit;
    }

    private static void SnapshotFrequencies(
        scrPlayer player,
        PlayerState state
    )
    {
        state.ObservedKeyFrequencies.Clear();
        foreach (var pair in player.keyFrequency)
            state.ObservedKeyFrequencies[pair.Key] = pair.Value;
    }

    private static LevelEvent FindLatestEvent(int floorIndex)
    {
        var game = scnGame.instance;
        if (game == null)
            return null;

        var events = game.events;
        if (events == null)
            return null;

        LevelEvent latest = null;
        var latestFloor = int.MinValue;

        foreach (var levelEvent in events)
        {
            if (levelEvent == null
                || !levelEvent.active
                || levelEvent.eventType != KeyLimitEventRegistry.EventType
                || levelEvent.floor > floorIndex
                || levelEvent.floor < latestFloor)
            {
                continue;
            }

            latest = levelEvent;
            latestFloor = levelEvent.floor;
        }

        return latest;
    }

    private static KeyLimitExceededAction GetExceededAction(
        LevelEvent sourceEvent
    )
    {
        if (sourceEvent == null)
            return KeyLimitExceededAction.Die;

        var value = sourceEvent[KeyLimitEventRegistry.ExceededActionPropertyName];
        if (value is KeyLimitExceededAction action)
            return action;

        return Enum.TryParse(
            value?.ToString(),
            true,
            out action
        )
            ? action
            : KeyLimitExceededAction.Die;
    }

    private static string GetFailMessage()
    {
        return RDString.language == SystemLanguage.Korean
            ? "키 제한을 초과했습니다."
            : "Key limit exceeded.";
    }
}
