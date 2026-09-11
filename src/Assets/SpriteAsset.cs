using System;
using System.IO;
using UnityEngine;

namespace KeyLimiter.Assets;

internal sealed class SpriteAsset : IDisposable
{
    private static readonly Func<Texture2D, byte[], bool, bool> LoadImage = CreateImageLoader();
    private readonly Texture2D texture;

    internal Sprite Sprite { get; }

    private SpriteAsset(Texture2D texture, Sprite sprite)
    {
        this.texture = texture;
        Sprite = sprite;
    }

    internal static bool TryLoad(string path, string name, out SpriteAsset asset)
    {
        asset = null;
        if (LoadImage == null || !File.Exists(path))
            return false;

        Texture2D texture = null;
        Sprite sprite = null;
        try
        {
            texture = new Texture2D(2, 2, TextureFormat.RGBA32, false)
            {
                name = name,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            if (!LoadImage(texture, File.ReadAllBytes(path), false))
            {
                UnityEngine.Object.Destroy(texture);
                return false;
            }

            sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                100f
            );
            sprite.name = name;
            asset = new SpriteAsset(texture, sprite);
            return true;
        }
        catch (Exception exception)
        {
            if (sprite != null)
                UnityEngine.Object.Destroy(sprite);
            if (texture != null)
                UnityEngine.Object.Destroy(texture);

            Debug.LogError($"[KeyLimiter] Failed to load image '{path}': {exception}");
            return false;
        }
    }

    public void Dispose()
    {
        if (Sprite != null)
            UnityEngine.Object.Destroy(Sprite);
        if (texture != null)
            UnityEngine.Object.Destroy(texture);
    }

    private static Func<Texture2D, byte[], bool, bool> CreateImageLoader()
    {
        // The game's image module targets netstandard 2.1, but UMM mods target .NET 4.8.
        // Resolve once and cache a typed delegate instead of reflecting on every load.
        try
        {
            var method = Type.GetType(
                "UnityEngine.ImageConversion, UnityEngine.ImageConversionModule"
            )?.GetMethod("LoadImage", [typeof(Texture2D), typeof(byte[]), typeof(bool)]);

            return method == null
                ? null
                : (Func<Texture2D, byte[], bool, bool>)Delegate.CreateDelegate(
                    typeof(Func<Texture2D, byte[], bool, bool>), method);
        }
        catch (Exception exception)
        {
            Debug.LogError($"[KeyLimiter] Image loader is unavailable: {exception}");
            return null;
        }
    }
}
