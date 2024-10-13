using System;
using System.Collections.Generic;

// ServiceLocator - класс для регистрации сервисов
namespace Utilities
{
    public static class ServiceLocator
    {
        // всё делает словарь, где ключ - тип, и объект - сервис
        private static readonly Dictionary<Type, object> _services = new();
        
        // метод регистрации сервиса
        public static void Register<T>(T service)
        {
            _services[typeof(T)] = service;
        }
        
        // тип по которому мы хотим зарегистрировать сервис
        public static void RegisterAs<T>(T service, Type type)
        {
            _services[type] = service;
        }
        
        // убрать регистрацию
        public static void Unregister<T>()
        {
            _services.Remove(typeof(T));
        }
        
        // есть ли сервис в словаре
        public static bool Contains<T>()
        {
            return _services.ContainsKey(typeof(T));
        }
        
        // получить сервис по типу - дженерик
        public static T Get<T>()
        {
            return (T) _services[typeof(T)];
        }
        
        // очистка словаря с сервисом
        public static void Clear()
        {
            _services.Clear();
        }
    }
}