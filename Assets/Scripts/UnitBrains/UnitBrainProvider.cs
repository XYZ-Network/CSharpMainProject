using System;
using System.Collections.Generic;
using System.Linq;
using Model.Config;
using UnityEditor.Compilation;
using UnityEngine;

namespace UnitBrains
{
    public static class UnitBrainProvider
    {
        private static readonly List<BaseUnitBrain> _brainsCache = new();
        
        public static BaseUnitBrain GetBrain(UnitConfig forUnit)
        {
            InitBrainsCache();

            var brain = _brainsCache.FirstOrDefault(b =>
                b.TargetUnitName == forUnit.Name && b.IsPlayerUnitBrain == forUnit.IsPlayerUnit);
            
            if (brain == null)
                brain = _brainsCache.FirstOrDefault(b =>
                    string.IsNullOrEmpty(b.TargetUnitName) && b.IsPlayerUnitBrain == forUnit.IsPlayerUnit);

            if (brain == null)
            {
                Debug.LogError($"Could not find brains for unit {forUnit.Name}");
                return null;
            }

            return (BaseUnitBrain) Activator.CreateInstance(brain.GetType());
        }

        private static void InitBrainsCache()
        {
            if (_brainsCache.Count != 0)
                return;
            
            _brainsCache.AddRange(
                AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => a.GetTypes())
                    .Where(t => !t.IsAbstract && typeof(BaseUnitBrain).IsAssignableFrom(t))
                    .Select(t => (BaseUnitBrain) Activator.CreateInstance(t))
            );
        }
    }
}