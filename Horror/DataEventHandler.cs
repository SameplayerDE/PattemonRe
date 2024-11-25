using System;
using System.Collections.Generic;
using Horror.Graphics;

namespace Horror;

public class DataEventHandler
{
    private readonly Dictionary<string, List<Action<DataEvent>>> _subscribers = new();
    
    public void Subscribe(string eventName, Action<DataEvent> listener)
    {
        if (!_subscribers.TryGetValue(eventName, out var value))
        {
            value = [];
            _subscribers[eventName] = value;
        }
        value.Add(listener);
    }

    public void Publish(DataEvent @event)
    {
        if (!_subscribers.TryGetValue(@event.Name, out var listeners))
        {
            return;
        }
        foreach (var listener in listeners)
        {
            listener(@event);
        }
    }

    public void Unsubscribe(string eventName, Action<DataEvent> listener)
    {
        if (!_subscribers.TryGetValue(eventName, out var listeners))
        {
            return;
        }
        listeners.Remove(listener);
        if (listeners.Count == 0)
        {
            _subscribers.Remove(eventName);
        }
    }
}