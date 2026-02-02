using System;
using System.Collections.Generic;

namespace Utilities
{
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> _services = new();
        private static readonly object _lock = new();
        
        public static void Register<T>(T service)
        {
            lock (_lock)
            {
                _services[typeof(T)] = service;
            }
        }
        
        public static void RegisterAs<T>(T service, Type type)
        {
            lock (_lock)
            {
                _services[type] = service;
            }
        }

        // Non-generic register by type
        public static void Register(Type type, object service)
        {
            lock (_lock)
            {
                _services[type] = service;
            }
        }

        // Non-generic register as specific key type
        public static void RegisterAs(object service, Type type)
        {
            lock (_lock)
            {
                _services[type] = service;
            }
        }
        
        public static void Unregister<T>()
        {
            lock (_lock)
            {
                _services.Remove(typeof(T));
            }
        }
        
        public static bool Contains<T>()
        {
            lock (_lock)
            {
                return _services.ContainsKey(typeof(T));
            }
        }

        // Non-generic Contains
        public static bool Contains(Type type)
        {
            lock (_lock)
            {
                return _services.ContainsKey(type);
            }
        }

        public static bool TryGet<T>(out T service)
        {
            lock (_lock)
            {
                if (_services.TryGetValue(typeof(T), out var obj) && obj is T typed)
                {
                    service = typed;
                    return true;
                }

                service = default;
                return false;
            }
        }
        
        public static T Get<T>()
        {
            lock (_lock)
            {
                return (T) _services[typeof(T)];
            }
        }

        // Non-generic TryGet
        public static bool TryGet(Type type, out object service)
        {
            lock (_lock)
            {
                return _services.TryGetValue(type, out service);
            }
        }

        // Non-generic Get
        public static object Get(Type type)
        {
            lock (_lock)
            {
                return _services[type];
            }
        }
        
        public static void Clear()
        {
            lock (_lock)
            {
                _services.Clear();
            }
        }
    }
}