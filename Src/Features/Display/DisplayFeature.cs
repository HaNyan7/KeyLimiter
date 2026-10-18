using KeyLimiter.Features.KeyLimit;
using KeyLimiter.Features.Localization;
using KeyLimiter.Features.Settings;
using UnityEngine;
using Strings = KeyLimiter.Localization.strings;

namespace KeyLimiter.Features.Display;

internal static class DisplayFeature
{
    private static GameObject displayObject;

    internal static void Enable()
    {
        if (displayObject != null)
            return;

        displayObject = new GameObject("KeyLimiterDisplay")
        {
            hideFlags = HideFlags.HideAndDontSave
        };

        Object.DontDestroyOnLoad(displayObject);
        displayObject.AddComponent<DisplayBehaviour>();
    }

    internal static void Disable()
    {
        if (displayObject != null)
            Object.Destroy(displayObject);

        displayObject = null;
    }
}

internal sealed class DisplayBehaviour : MonoBehaviour
{
    private GUIStyle textStyle;

    private void OnGUI()
    {
        if (!ModSettingsFeature.Settings.Enable
            || scnGame.instance == null)
        {
            return;
        }

        textStyle ??= new GUIStyle(GUI.skin.label)
        {
            fontSize = 24,
            normal = new GUIStyleState
            {
                textColor = Color.white
            }
        };

        GUI.Label(
            new Rect(20f, 20f, 300f, 40f),
            LocalizationFeature.Get(
                nameof(Strings.ModDisplay),
                KeyLimitRuntime.AvailableKeys
            ),
            textStyle
        );
    }
}
