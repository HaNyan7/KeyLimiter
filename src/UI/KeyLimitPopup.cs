using KeyLimiter.Assets;
using KeyLimiter.Gameplay;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace KeyLimiter.UI;

internal static class KeyLimitPopup
{
    private const float HoldDuration = 0.7f;
    private const float FadeDuration = 2.3f;
    private const int MaximumImageCount = 24;

    private static readonly Dictionary<int, SpriteAsset> images = [];
    private static readonly HashSet<int> missingImages = [];

    private static string modDirectory = string.Empty;
    private static scrHitErrorMeter owner;
    private static Image popupImage;
    private static float fadeStartTime;

    internal static void SetModDirectory(string path)
    {
        modDirectory = path ?? string.Empty;
    }

    internal static void Update(scrController controller)
    {
        if (controller == null || controller.errorMeter == null)
        {
            Hide();
            return;
        }

        EnsureImage(controller.errorMeter);
        if (popupImage == null)
            return;

        if (KeyLimitRuntime.TryTakePopup(controller.playerOne, out var limit))
            Show(limit);

        if (!popupImage.gameObject.activeSelf)
            return;

        var elapsed = Time.unscaledTime - fadeStartTime;
        if (elapsed >= FadeDuration)
        {
            Hide();
            return;
        }

        if (elapsed > 0f)
        {
            var color = popupImage.color;
            color.a = 1f - elapsed / FadeDuration;
            popupImage.color = color;
        }
    }

    internal static void Reset()
    {
        DestroyImage();
        foreach (var image in images.Values)
            image.Dispose();

        images.Clear();
        missingImages.Clear();
    }

    private static void EnsureImage(scrHitErrorMeter meter)
    {
        if (popupImage != null && owner == meter)
            return;

        DestroyImage();
        var canvas = meter.GetComponentInParent<Canvas>();
        if (canvas == null)
            return;

        owner = meter;
        var popupObject = new GameObject(
            "KeyLimiterCountPopup",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );
        popupObject.transform.SetParent(canvas.transform, false);

        var rect = popupObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.25f);
        rect.anchorMax = rect.anchorMin;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;

        popupImage = popupObject.GetComponent<Image>();
        popupImage.preserveAspect = true;
        popupImage.raycastTarget = false;
        popupObject.SetActive(false);
    }

    private static void Show(int limit)
    {
        if (!TryGetImage(limit, out var image))
        {
            Hide();
            return;
        }

        popupImage.sprite = image.Sprite;
        popupImage.SetNativeSize();
        popupImage.color = Color.white;
        popupImage.transform.SetAsLastSibling();
        popupImage.gameObject.SetActive(true);
        fadeStartTime = Time.unscaledTime + HoldDuration;
    }

    private static bool TryGetImage(int limit, out SpriteAsset image)
    {
        if (images.TryGetValue(limit, out image))
            return true;

        if (limit < 1 || limit > MaximumImageCount || missingImages.Contains(limit))
            return false;

        var path = Path.Combine(modDirectory, "decorations", $"{limit}.png");
        if (!SpriteAsset.TryLoad(path, $"KeyLimiterCount{limit}", out image))
        {
            missingImages.Add(limit);
            return false;
        }

        images[limit] = image;
        return true;
    }

    private static void DestroyImage()
    {
        if (popupImage != null)
            UnityEngine.Object.Destroy(popupImage.gameObject);

        owner = null;
        popupImage = null;
        fadeStartTime = 0f;
    }

    private static void Hide()
    {
        if (popupImage != null && popupImage.gameObject.activeSelf)
            popupImage.gameObject.SetActive(false);
    }
}
