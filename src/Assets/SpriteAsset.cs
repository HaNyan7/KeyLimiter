using System;
using System.IO;
using UnityEngine;

namespace KeyLimiter.Assets;

internal sealed class SpriteAsset : IDisposable
{
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
        if (!File.Exists(path))
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

            if (!ImageConversion.LoadImage(texture, File.ReadAllBytes(path)))
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
}
