using KeyLimiter.KeyLimit;
using KeyLimiter.Localization;
using KeyLimiter.Settings;
using UnityEngine;
using Strings = KeyLimiter.Localization.strings;

namespace KeyLimiter.Display;

internal static class DisplayController
{
    private static GameObject display;

    internal static void Create()
    {
        if (display != null) return;

        display = new GameObject("KeyLimiterDisplay")
        {
            hideFlags = HideFlags.HideAndDontSave
        };

        Object.DontDestroyOnLoad(display);
        display.AddComponent<DisplayBehaviour>();
        display.SetActive(false);
    }

    internal static void SetVisible(bool visible)
    {
        if (display == null)
        {
            if (!visible)
                return;

            Create();
        }

        display.SetActive(visible);
    }

    internal static void Destroy()
    {
        if (display != null)
            Object.Destroy(display);

        display = null;
    }
}

internal sealed class DisplayBehaviour : MonoBehaviour
{
    private GUIStyle textStyle;

    private void OnGUI()
    {
        textStyle ??= new GUIStyle(GUI.skin.label)
        {
            fontSize = 24,
            alignment = TextAnchor.MiddleCenter,
            richText = true,
            normal = new GUIStyleState
            {
                textColor = Color.white
            }
        };

        GUI.Label(
            new Rect(
                0f,
                Screen.height * 0.25f,
                Screen.width,
                Screen.height
            ),
            $"Key Limit <b>{KeyState.AvailableKeys}</b>",
            textStyle
        );
    }
}
