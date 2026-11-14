using KeyLimiter.Localization;
using KeyLimiter.Settings;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Strings = KeyLimiter.Localization.strings;

namespace KeyLimiter.KeyLimit;

public static class KeyLimitRuntime
{
    private sealed class PlayerState
    {
        internal readonly Dictionary<object, int> KeyFrequencies = [];
    }

    private static ConditionalWeakTable<scrPlayer, PlayerState> playerStates = new();

    private static readonly HashSet<object> pressedKeys = [];
    private static readonly HashSet<object> storedKeys = [];

    public static ISet<object> PressedKeys => pressedKeys;
    public static ISet<object> StoredKeys => storedKeys;

    public static int AvailableKeys => Math.Max(
        0,
        ModSettingsController.Settings.AvailableKeys - storedKeys.Count
    );

    /// <summary>
    /// 입력 처리 전 PressedKeys를 초기화하고,
    /// 현재 키 입력 빈도를 상태로 저장
    /// </summary>
    /// <param name="player">
    /// 입력 상태를 저장할 플레이어 객체
    /// </param>
    internal static void PrefixInput(scrPlayer player)
    {
        if (!ModSettingsController.Settings.Enable
            || player == null
            || !player.alive) return;

        var state = playerStates.GetValue(
            player,
            CreatePlayerState
        );

        PressedKeys.Clear();

        state.KeyFrequencies.Clear();

        foreach (var pair in player.keyFrequency)
        {
            state.KeyFrequencies[pair.Key] = pair.Value;
        }
    }

    /// <summary>
    /// 입력 처리 후 키 입력 빈도의 증가 여부를 검사하고,
    /// 입력 제한 조건에 따라 키 입력을 저장하거나 초과 동작을 수행
    /// </summary>
    /// <param name="player">
    /// 입력 상태를 검사할 플레이어 객체
    /// </param>
    /// <returns>
    /// 입력 처리가 허용되면 true, 제한을 초과하면 false를 반환
    /// </returns>
    internal static bool PostfixInput(scrPlayer player)
    {
        if (!ModSettingsController.Settings.Enable)
        {
            Reset(player);
            return true;
        }

        if (player == null || !player.alive) return true;

        var state = playerStates.GetValue(
            player,
            CreatePlayerState
        );

        foreach (var pair in player.keyFrequency)
        {
            state.KeyFrequencies.TryGetValue(
                pair.Key,
                out var previousCount
            );

            if (pair.Value > previousCount) pressedKeys.Add(pair.Key);
        }

        if (CanCommit())
        {
            storedKeys.UnionWith(pressedKeys);
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

    /// <summary>
    /// 지정된 플레이어의 상태를 초기화
    /// </summary>
    /// <param name="player">
    /// 상태를 초기화할 플레이어 객체
    /// </param>
    internal static void Reset(scrPlayer player)
    {
        if (player != null)
            playerStates.Remove(player);

        pressedKeys.Clear();
        storedKeys.Clear();
    }

    /// <summary>
    /// 모든 플레이어 상태를 초기화하고 키 제한 상태를 재설정
    /// </summary>
    internal static void ResetAll()
    {
        playerStates =
            new ConditionalWeakTable<scrPlayer, PlayerState>();

        pressedKeys.Clear();
        storedKeys.Clear();
    }

    /// <summary>
    /// 제한을 넘지 않는지 검사
    /// </summary>
    /// <returns>허용 여부</returns>
    private static bool CanCommit()
    {
        var newKeyCount = 0;

        foreach (var key in pressedKeys)
        {
            if (!storedKeys.Contains(key))
                newKeyCount++;
        }

        return storedKeys.Count + newKeyCount <= ModSettingsController.Settings.AvailableKeys;
    }

    private static PlayerState CreatePlayerState(scrPlayer _)
    {
        return new PlayerState();
    }
}