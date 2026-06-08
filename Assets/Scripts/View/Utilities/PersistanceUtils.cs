using System;
using UnityEngine;

namespace Utilities
{
    public static class PersistanceUtils
    {
        public static void PersistSingleton(object singleton)
        {
            var typeName = singleton.GetType().Name;
            var json = JsonUtility.ToJson(singleton);
            PlayerPrefs.SetString(typeName, json);
        }
        
        public static T LoadSingleton<T>(T defaultValue)
        {
            var typeName = typeof(T).Name;
            string json = string.Empty;
            if (!PlayerPrefs.HasKey(typeName) || string.IsNullOrEmpty(json = PlayerPrefs.GetString(typeName)))
            {
                return defaultValue;
            }

            try
            {
                return JsonUtility.FromJson<T>(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load singleton {typeName} from json {json} with error {e}");
                return defaultValue;
            }
        }
    }
}