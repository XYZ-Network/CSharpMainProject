using Model;
using System.Collections.Generic;
using UnityEngine;
using View;

namespace Utilities
{
    public static class ServiceLocator
    {
        private static readonly Dictionary<System.Type, object> _services = new();

        public static void Register<T>(T service)
        {
            _services[typeof(T)] = service;
        }

        public static void RegisterAs<T>(T service, System.Type type)
        {
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
            return (T)_services[typeof(T)];
        }

        public static void Clear()
        {
            _services.Clear();
        }

        static ServiceLocator()
        {
            Register(new RuntimeModel());
            Register(new VFXView());
            Register(CoroutineManager.Instance);
        }
    }
}
