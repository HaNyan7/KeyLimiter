using KeyLimiter.KeyLimit;
using KeyLimiter.Settings;
using UnityEngine;

namespace KeyLimiter.Display;

// TODO: 폰트 수정, 코드 최적화
internal static class DisplayController
{
    private static GameObject display;
    private static bool playing;

    internal static bool Playing =>
        playing
        && scrController.instance != null
        && scrController.instance.currentState != States.Fail
        && scrController.instance.currentState != States.Fail2
        && scrController.instance.currentState != States.Won
        && !scrController.instance.paused;

    internal static void Create()
    {
        if (display != null) return;

        display = new GameObject("KeyLimiterDisplay")
        {
            hideFlags = HideFlags.HideAndDontSave
        };

        Object.DontDestroyOnLoad(display);
        display.AddComponent<DisplayBehaviour>();
    }

    internal static void Play()
    {
        playing = true;
    }

    internal static void Stop()
    {
        playing = false;
    }

    internal static void Destroy()
    {
        if (display != null)
            Object.Destroy(display);

        display = null;
        playing = false;
    }
}

internal sealed class DisplayBehaviour : MonoBehaviour
{
    private GUIStyle textStyle;

    private void OnGUI()
    {
        if (!DisplayController.Playing
            || !ModSettingsController.Settings.Enable)
        {
            return;
        }

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

        var text = $"KeyLimiter {KeyLimitRuntime.AvailableKeys}";

        textStyle.normal.textColor = new Color(0f, 0f, 0f, 0.55f);

        GUI.Label(
            new Rect(
                2f,
                Screen.height * 0.75f - 18f,
                Screen.width,
                40f
            ),
            text,
            textStyle
        );

        textStyle.normal.textColor = Color.white;

        GUI.Label(
            new Rect(
                0f,
                Screen.height * 0.75f - 20f,
                Screen.width,
                40f
            ),
            text,
            textStyle
        );
    }
}
