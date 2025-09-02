using System;
using System.Collections.Generic;
using UnityEngine;

public class EventBusInternal
{
    private readonly Dictionary<Type, object> subscribers = new();

    public void Subscribe<T>(Action<T> handler)
        where T : struct, IGameEvent
    {
        Type type = typeof(T);
        if (!subscribers.TryGetValue(type, out object handlers))
            subscribers[type] = new List<Action<T>>();

        ((List<Action<T>>)subscribers[type]).Add(handler);
    }

    public void Unsubscribe<T>(Action<T> handler)
        where T : struct, IGameEvent
    {
        Type type = typeof(T);
        if (subscribers.TryGetValue(type, out object handlers))
            ((List<Action<T>>)subscribers[type]).Remove(handler);
    }

    public void Publish<T>(T eventData)
        where T : struct, IGameEvent
    {
        Type type = typeof(T);
        if (subscribers.TryGetValue(type, out object handlers))
        {
            var handlersCopy = new List<Action<T>>((List<Action<T>>)handlers);
            foreach (var handler in handlersCopy)
                handler?.Invoke(eventData);
        }
    }
}

public static class EventBus
{
    public static readonly EventBusInternal Instance = new();
}

public interface IGameEvent { }
