using KeyLimiter.Features.KeyLimit;
using KeyLimiter.Features.Settings;
using UnityEngine;

namespace KeyLimiter.Features.Hud;

internal static class HudFeature
{
    private static GameObject hudObject;

    internal static void Enable()
    {
        if (hudObject != null)
            return;

        hudObject = new GameObject("KeyLimiterHud")
        {
            hideFlags = HideFlags.HideAndDontSave
        };

        Object.DontDestroyOnLoad(hudObject);
        hudObject.AddComponent<HudBehaviour>();
    }

    internal static void Disable()
    {
        if (hudObject != null)
            Object.Destroy(hudObject);

        hudObject = null;
    }
}

internal sealed class HudBehaviour : MonoBehaviour
{
    private GUIStyle textStyle;

    private void OnGUI()
    {
        if (!ModSettingsFeature.Settings.ShowHud
            || !ModSettingsFeature.Settings.Enable
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
            RDString.language == SystemLanguage.Korean
                ? $"남은 키 {KeyLimitRuntime.RemainingKeyCount}"
                : $"Remaining Keys {KeyLimitRuntime.RemainingKeyCount}",
            textStyle
        );
    }
}
