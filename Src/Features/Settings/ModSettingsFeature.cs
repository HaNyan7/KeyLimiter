using KeyLimiter.Features.Localization;
using UnityEngine;
using UnityModManagerNet;
using Strings = KeyLimiter.Localization.strings;

namespace KeyLimiter.Features.Settings;

internal static class ModSettingsFeature
{
    private static string availableKeysInput = string.Empty;

    internal static ModSettings Settings { get; private set; } = new();

    internal static void Load(UnityModManager.ModEntry modEntry)
    {
        Settings = UnityModManager.ModSettings.Load<ModSettings>(modEntry)
            ?? new ModSettings();

        availableKeysInput = Settings.AvailableKeys.ToString();
    }

    internal static void Draw()
    {
        Settings.Enable = GUILayout.Toggle(
            Settings.Enable,
            LocalizationFeature.Get(nameof(Strings.SettingsEnable))
        );

        if (Settings.Enable)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(24f);
            GUILayout.BeginVertical();

            // 사용 가능한 키 시작
            GUILayout.BeginHorizontal();

            GUILayout.Label(
                $"{LocalizationFeature.Get(nameof(Strings.SettingsAvailableKeys))}: ",
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
                LocalizationFeature.Get(nameof(Strings.SettingsExceededAction)),
                GUILayout.ExpandWidth(false)
            );

            Settings.ExceededAction =
                (KeyLimitExceededAction)GUILayout.Toolbar(
                    (int)Settings.ExceededAction,
                    [
                        LocalizationFeature.Get(nameof(Strings.ExceededActionDie)),
                        LocalizationFeature.Get(nameof(Strings.ExceededActionIgnore))
                    ],
                    GUILayout.ExpandWidth(false)
                );

            GUILayout.EndHorizontal();
            // 액션 끝

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.Space(12f);
        }
    }

    internal static void Save(UnityModManager.ModEntry modEntry)
    {
        Settings.Save(modEntry);
    }
}
