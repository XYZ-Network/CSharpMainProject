using System;
using System.Collections.Generic;
using System.Linq;
using Model;
using Model.Runtime.Projectiles;
using UnityEngine;
using Utilities;
using UnitBrains.Player;
using Model.Runtime;
using Utilities;

namespace Utilities
{
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> _services = new();

        public static void Register<T>(T service)
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service), "Service cannot be null.");
            }
            if (_services.ContainsKey(typeof(T)))
            {
                Debug.LogWarning($"Service of type {typeof(T)} is already registered. Overwriting.");
            }
            _services[typeof(T)] = service;
            Debug.Log($"Service of type {typeof(T)} registered successfully.");
        }

        public static void RegisterAs<T>(T service, Type type)
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service), "Service cannot be null.");
            }
            _services[type] = service;
        }

        public static void Unregister<T>()
        {
            _services.Remove(typeof(T));
        }

        public static bool Contains<T>()
        {
            return _services.ContainsKey(typeof(T));
        }

        public static T Get<T>()
        {
            if (_services.TryGetValue(typeof(T), out var service))
            {
                if (service is T typedService)
                {
                    Debug.Log($"Service of type {typeof(T)} retrieved successfully.");
                    return typedService;
                }
                throw new InvalidCastException($"Service registered as {service.GetType()} cannot be cast to {typeof(T)}.");
            }
            throw new KeyNotFoundException($"Service of type {typeof(T)} is not registered.");
        }

        public static void Clear()
        {
            _services.Clear();
        }
    }
}