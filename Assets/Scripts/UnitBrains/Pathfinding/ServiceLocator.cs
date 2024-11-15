using System;
using System.Collections.Generic;
using System.Linq;
using Model.Runtime;
using Model.Runtime.Projectiles;
using Model.Runtime.ReadOnly;
using UnityEngine;

public static class Locator
{
    private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

    public static void Register<T>(T service)
    {
        var type = typeof(T);
        if (_services.ContainsKey(type))
        {
            _services[type] = service; // обновление существующего сервиса
        }
        else
        {
            _services.Add(type, service); // добавление нового сервиса
        }
    }

    public static T Get<T>()
    {
        var type = typeof(T);
        if (_services.TryGetValue(type, out var service))
        {
            return (T)service;
        }

        throw new Exception($"Service of type {type} not registered.");
    }

    public static void Unregister<T>()
    {
        var type = typeof(T);
        if (_services.ContainsKey(type))
        {
            _services.Remove(type);
        }
    }
}