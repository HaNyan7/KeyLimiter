using KeyLimiter.KeyLimit;
using KeyLimiter.Settings;
using UnityEngine;

namespace KeyLimiter.Display;

// TODO: 폰트 수정, 코드 최적화
internal static class DisplayController
{
    private static Font interFont;
    internal static Font InterFont => interFont;

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
        if (interFont == null)
        {
            foreach (var font in Resources.FindObjectsOfTypeAll<Font>())
            {
                if (font != null && font.name == "Inter-Light")
                {
                    interFont = font;
                    break;
                }
            }

            if (interFont == null)
            {
                Debug.LogWarning(
                    "[KeyLimiter] Inter-Light font was not found."
                );
            }
        }

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
            fontStyle = FontStyle.Normal,
            alignment = TextAnchor.MiddleCenter,
            richText = true,
            normal = new GUIStyleState
            {
                textColor = Color.white
            }
        };

        textStyle.font = DisplayController.InterFont ?? GUI.skin.font;

        var text = $"KeyLimit {KeyLimitRuntime.AvailableKeys}";

        // 그림자
        var textRect = new Rect(
            0f,
            Screen.height * 0.75f - 20f,
            Screen.width,
            40f
        );

        const int shadowRadius = 4;
        const float shadowOffsetX = 1f;
        const float shadowOffsetY = 2f;
        const float shadowSigma = 2f;

        if (Event.current.type == EventType.Repaint)
        {
            for (var x = -shadowRadius; x <= shadowRadius; x++)
            {
                for (var y = -shadowRadius; y <= shadowRadius; y++)
                {
                    var distanceSquared = x * x + y * y;

                    if (distanceSquared == 0
                        || distanceSquared > shadowRadius * shadowRadius)
                    {
                        continue;
                    }

                    var alpha = Mathf.Exp(
                        -distanceSquared
                        / (2f * shadowSigma * shadowSigma)
                    ) * 0.09f;

                    textStyle.normal.textColor =
                        new Color(0f, 0f, 0f, alpha);

                    GUI.Label(
                        new Rect(
                            textRect.x + shadowOffsetX + x,
                            textRect.y + shadowOffsetY + y,
                            textRect.width,
                            textRect.height
                        ),
                        text,
                        textStyle
                    );
                }
            }
        }

        textStyle.normal.textColor = Color.white;

        GUI.Label(
            textRect,
            text,
            textStyle
        );
    }
}
