using UnityEngine;
using UnityModManagerNet;

namespace KeyLimiter.Features.Settings;

internal static class ModSettingsFeature
{
    private static string limitCountInput = string.Empty;

    internal static ModSettings Settings { get; private set; } = new();

    internal static void Load(UnityModManager.ModEntry modEntry)
    {
        Settings = UnityModManager.ModSettings.Load<ModSettings>(modEntry)
            ?? new ModSettings();

        limitCountInput = Settings.LimitCount.ToString();
    }

    internal static void Draw()
    {
        var localizedText = RDString.language switch
        {
            SystemLanguage.Korean => new
            {
                Enable = "키 제한",
                LimitCount = "키 제한 수",
                ExceededAction = "초과 시 액션",
                Die = "사망",
                Ignore = "입력 무시",
                ShowHud = "HUD 표시"
            },

            _ => new
            {
                Enable = "Key Limiter",
                LimitCount = "Key Limit Count",
                ExceededAction = "On Limit Exceeded",
                Die = "Die",
                Ignore = "Ignore Input",
                ShowHud = "Show HUD"
            }
        };

        Settings.Enable = GUILayout.Toggle(
            Settings.Enable,
            localizedText.Enable
        );

        if (Settings.Enable)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(24f);
            GUILayout.BeginVertical();

            // 제한 수 시작
            GUILayout.BeginHorizontal();

            GUILayout.Label(
                $"{localizedText.LimitCount}: ",
                GUILayout.ExpandWidth(false)
            );

            var sliderValue = Mathf.RoundToInt(
                GUILayout.HorizontalSlider(
                    Settings.LimitCount,
                    1f,
                    40f,
                    GUILayout.Width(250f)
                )
            );

            var inputValue = GUILayout.TextField(
                limitCountInput,
                GUILayout.Width(50f)
            );

            if (sliderValue != Settings.LimitCount)
            {
                Settings.LimitCount = sliderValue;
                limitCountInput = sliderValue.ToString();
            }
            else if (inputValue != limitCountInput)
            {
                limitCountInput = inputValue;

                // 값 보정
                if (int.TryParse(inputValue, out var parsedValue))
                {
                    Settings.LimitCount = Mathf.Clamp(parsedValue, 1, 40);

                    if (parsedValue != Settings.LimitCount)
                    {
                        limitCountInput = Settings.LimitCount.ToString();
                    }
                }
            }

            GUILayout.EndHorizontal();
            // 제한 수 끝

            // 액션 시작
            GUILayout.BeginHorizontal();

            GUILayout.Label(
                localizedText.ExceededAction,
                GUILayout.ExpandWidth(false)
            );

            Settings.ExeededAction =
                (KeyLimitExceededAction)GUILayout.Toolbar(
                    (int)Settings.ExeededAction,
                    [
                        localizedText.Die,
                        localizedText.Ignore
                    ],
                    GUILayout.ExpandWidth(false)
                );

            GUILayout.EndHorizontal();
            // 액션 끝

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.Space(12f);
        }

        Settings.ShowHud = GUILayout.Toggle(
            Settings.ShowHud,
            localizedText.ShowHud
        );
    }

    internal static void Save(UnityModManager.ModEntry modEntry)
    {
        Settings.Save(modEntry);
    }
}
