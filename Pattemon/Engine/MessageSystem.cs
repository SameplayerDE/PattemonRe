namespace Pattemon.Engine;

using System;
using System.Collections.Generic;

public static class MessageSystem
{
    private static readonly Dictionary<string, Action<object?>> _events = new();

    public static void Subscribe(string eventName, Action<object?> callback)
    {
        if (!_events.ContainsKey(eventName))
        {
            _events[eventName] = _ => { };
        }
        _events[eventName] += callback;
    }

    public static void Unsubscribe(string eventName, Action<object?> callback)
    {
        if (_events.ContainsKey(eventName))
        {
            _events[eventName] -= callback;
        }
    }

    public static void Publish(string eventName, object? data = null)
    {
        if (_events.TryGetValue(eventName, out var @event))
        {
            @event.Invoke(data);
        }
    }
}
