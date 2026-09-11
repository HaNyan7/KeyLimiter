using ADOFAI;
using KeyLimiter.Assets;
using KeyLimiter.Gameplay;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace KeyLimiter.Events;

internal static class KeyLimitEventRegistry
{
    internal const string EventName = "KeyLimiter";
    internal const string CountPropertyName = "key-limit-count";
    internal const string ExceededActionPropertyName = "on-limit-exceeded";
    internal const int TypeId = 900;

    private static readonly string NumericTypeName =
        TypeId.ToString(CultureInfo.InvariantCulture);

    private static LevelEventInfo registeredInfo;
    private static Sprite registeredIcon;
    private static SpriteAsset customIcon;
    private static string modDirectory = string.Empty;

    internal static LevelEventType EventType => (LevelEventType)TypeId;

    internal static void SetModDirectory(string path)
    {
        modDirectory = path ?? string.Empty;
    }

    internal static void Register()
    {
        var events = GCS.levelEventsInfo;
        var typeNames = GCS.levelEventTypeString;
        if (events == null || typeNames == null)
            return;

        if (events.TryGetValue(EventName, out var existingInfo))
        {
            if (existingInfo.type != EventType)
                return;

            registeredInfo = existingInfo;
            typeNames[EventType] = EventName;
            return;
        }

        registeredInfo = CreateEventInfo();
        typeNames[EventType] = EventName;
        events[EventName] = registeredInfo;
    }

    internal static void EnsureIcon()
    {
        var icons = GCS.levelEventIcons;
        if (icons == null || icons.ContainsKey(EventType))
            return;

        if (!TryLoadCustomIcon(out var icon) && !TryGetFallbackIcon(icons, out icon))
            return;

        icons[EventType] = icon;
        registeredIcon = icon;
    }

    internal static void Unregister()
    {
        var events = GCS.levelEventsInfo;
        if (registeredInfo != null
            && events != null
            && events.TryGetValue(EventName, out var info)
            && ReferenceEquals(info, registeredInfo))
        {
            events.Remove(EventName);
        }

        var typeNames = GCS.levelEventTypeString;
        if (typeNames != null
            && typeNames.TryGetValue(EventType, out var eventName)
            && eventName == EventName)
        {
            typeNames.Remove(EventType);
        }

        var icons = GCS.levelEventIcons;
        if (icons != null
            && icons.TryGetValue(EventType, out var icon)
            && ReferenceEquals(icon, registeredIcon))
        {
            icons.Remove(EventType);
        }

        registeredInfo = null;
        registeredIcon = null;
        customIcon?.Dispose();
        customIcon = null;
    }

    internal static LevelEventInfo GetEventInfo(
        Dictionary<string, LevelEventInfo> source,
        string eventName
    )
    {
        return source[eventName == NumericTypeName ? EventName : eventName];
    }

    private static LevelEventInfo CreateEventInfo()
    {
        var info = new LevelEventInfo
        {
            type = EventType,
            name = EventName,
            executionTime = LevelEventExecutionTime.Special,
            categories = [LevelEventCategory.FxModifiers],
            propertiesInfo = []
        };

        AddProperty(info, new Dictionary<string, object>
        {
            ["name"] = CountPropertyName,
            ["type"] = "Int",
            ["default"] = 1,
            ["min"] = 0,
            ["max"] = 100,
            ["key"] = $"editor.{EventName}.{CountPropertyName}"
        });

        AddProperty(info, new Dictionary<string, object>
        {
            ["name"] = ExceededActionPropertyName,
            ["type"] = $"Enum:{typeof(KeyLimitExceededAction).AssemblyQualifiedName}",
            ["default"] = nameof(KeyLimitExceededAction.Die),
            ["key"] = $"editor.{EventName}.{ExceededActionPropertyName}"
        });

        return info;
    }

    private static void AddProperty(
        LevelEventInfo eventInfo,
        Dictionary<string, object> definition
    )
    {
        var propertyInfo = new PropertyInfo(definition, eventInfo);
        eventInfo.propertiesInfo.Add(propertyInfo.name, propertyInfo);
    }

    private static bool TryGetFallbackIcon(
        Dictionary<LevelEventType, Sprite> icons,
        out Sprite icon
    )
    {
        if (icons.TryGetValue(LevelEventType.SetInputEvent, out icon)
            || icons.TryGetValue(LevelEventType.SetConditionalEvents, out icon)
            || icons.TryGetValue(LevelEventType.SetSpeed, out icon))
        {
            return true;
        }

        foreach (var fallback in icons.Values)
        {
            icon = fallback;
            return true;
        }

        icon = null;
        return false;
    }

    private static bool TryLoadCustomIcon(out Sprite icon)
    {
        icon = customIcon?.Sprite;
        if (icon != null)
            return true;

        if (string.IsNullOrEmpty(modDirectory)
            || !SpriteAsset.TryLoad(
                Path.Combine(modDirectory, "icon.png"), "KeyLimiterIcon", out customIcon))
        {
            return false;
        }

        icon = customIcon.Sprite;
        return true;
    }
}
