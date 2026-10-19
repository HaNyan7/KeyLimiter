using KeyLimiter.KeyLimit;
using KeyLimiter.Localization;
using UnityEngine;
using UnityModManagerNet;
using Strings = KeyLimiter.Localization.strings;

namespace KeyLimiter.Settings;

internal static class ModSettingsController
{
    private static string availableKeysInput = string.Empty;

    internal static ModSettings Settings { get; private set; } = new();

    internal static void Load(UnityModManager.ModEntry modEntry)
    {
        Settings = UnityModManager.ModSettings.Load<ModSettings>(modEntry)
            ?? new ModSettings();

        availableKeysInput = Settings.AvailableKeys.ToString();
    }

    internal static bool Draw()
    {
        var previousEnable = Settings.Enable;
        var previousAvailableKeys = Settings.AvailableKeys;

        Settings.Enable = GUILayout.Toggle(
            Settings.Enable,
            Localizer.Get(nameof(Strings.SettingsEnable))
        );

        if (Settings.Enable)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(24f);
            GUILayout.BeginVertical();

            // 사용 가능한 키 시작
            GUILayout.BeginHorizontal();

            GUILayout.Label(
                $"{Localizer.Get(nameof(Strings.SettingsAvailableKeys))}: ",
                GUILayout.ExpandWidth(false)
            );

            var sliderValue = Mathf.RoundToInt(
                GUILayout.HorizontalSlider(
                    Settings.AvailableKeys,
                    1f,
                    40f,
                    GUILayout.Width(250f)
                )
            );

            var inputValue = GUILayout.TextField(
                availableKeysInput,
                GUILayout.Width(50f)
            );

            if (sliderValue != Settings.AvailableKeys)
            {
                Settings.AvailableKeys = sliderValue;
                availableKeysInput = sliderValue.ToString();
            }
            else if (inputValue != availableKeysInput)
            {
                availableKeysInput = inputValue;

                // 값 보정
                if (int.TryParse(inputValue, out var parsedValue))
                {
                    Settings.AvailableKeys = Mathf.Clamp(parsedValue, 1, 40);

                    if (parsedValue != Settings.AvailableKeys)
                    {
                        availableKeysInput = Settings.AvailableKeys.ToString();
                    }
                }
            }

            GUILayout.EndHorizontal();
            // 사용 가능한 키 끝

            // 액션 시작
            GUILayout.BeginHorizontal();

            GUILayout.Label(
                Localizer.Get(nameof(Strings.SettingsExceededAction)),
                GUILayout.ExpandWidth(false)
            );

            Settings.ExceededAction =
                (KeyLimitExceededAction)GUILayout.Toolbar(
                    (int)Settings.ExceededAction,
                    [
                        Localizer.Get(nameof(Strings.ExceededActionDie)),
                        Localizer.Get(nameof(Strings.ExceededActionIgnore))
                    ],
                    GUILayout.ExpandWidth(false)
                );

            GUILayout.EndHorizontal();
            // 액션 끝

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.Space(12f);
        }

        return previousEnable != Settings.Enable
            || previousAvailableKeys != Settings.AvailableKeys;
    }

    internal static void Save(UnityModManager.ModEntry modEntry)
    {
        Settings.Save(modEntry);
    }
}
