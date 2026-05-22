using Model;
using Model.Config;
using Model.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnitBrains
{
    public static class UnitBrainProvider
    {
        private static readonly List<BaseUnitBrain> _brainsCache = new();
        private static readonly Dictionary<int, PlayerCoordinator> _coordinators = new();

        public static void SetCoordinator(int playerId, PlayerCoordinator coordinator)
        {
            if (_coordinators.ContainsKey(playerId))
                _coordinators[playerId] = coordinator;
            else
                _coordinators.Add(playerId, coordinator);
        }

        public static BaseUnitBrain GetBrain(UnitConfig forUnit)
        {
            InitBrainsCache();

            var brainPrototype = _brainsCache.FirstOrDefault(b =>
                b.TargetUnitName == forUnit.Name &&
                b.IsPlayerUnitBrain == forUnit.IsPlayerUnit);

            if (brainPrototype == null)
            {
                brainPrototype = _brainsCache.FirstOrDefault(b =>
                    string.IsNullOrEmpty(b.TargetUnitName) &&
                    b.IsPlayerUnitBrain == forUnit.IsPlayerUnit);
            }

            if (brainPrototype == null)
            {
                Debug.LogError($"Could not find brains for unit {forUnit.Name}");
                return null;
            }

            var brain = (BaseUnitBrain)Activator.CreateInstance(brainPrototype.GetType());

            int playerId = forUnit.IsPlayerUnit
                ? RuntimeModel.PlayerId
                : RuntimeModel.BotPlayerId;

            if (_coordinators.TryGetValue(playerId, out var coordinator))
                brain.SetCoordinator(coordinator);

            return brain;
        }

        private static void InitBrainsCache()
        {
            if (_brainsCache.Count != 0)
                return;

            _brainsCache.AddRange(
                AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => a.GetTypes())
                    .Where(t => !t.IsAbstract && typeof(BaseUnitBrain).IsAssignableFrom(t))
                    .Select(t => (BaseUnitBrain)Activator.CreateInstance(t))
            );
        }
    }
}