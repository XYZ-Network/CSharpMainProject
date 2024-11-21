using System;
using System.Collections.Generic;
using System.Linq;
using Model;
using Model.Runtime.Projectiles;
using UnitBrains.Pathfinding;
using UnityEngine;

namespace UnitBrains.Player
{
    public class SecondUnitBrain : DefaultPlayerUnitBrain
    {
        private static int _unitsCounter = 0;
        private const int MaxTargets = 3;
        public override string TargetUnitName => "Cobra Commando";
        private const float OverheatTemperature = 3f;
        private const float OverheatCooldown = 2f;
        private float _temperature = 0f;
        private float _cooldownTime = 0f;
        private bool _overheated;
        private List<Vector2Int> _dangerousTargets = new();
        public int UnitID { get; private set; }

        public SecondUnitBrain()
        {
            UnitID = _unitsCounter++;
        }

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            if (GetTemperature() >= OverheatTemperature)
            {
                return; 
            }

          
            for (int i = 0; i <= GetTemperature(); i++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }
            IncreaseTemperature(); 
        }

        public override Vector2Int GetNextStep()
        {
            if (_dangerousTargets.Count == 0)
            {
                return unit.Pos; // Если нет опасных целей, остаемся на месте
            }

            var targetNumber = GetTargetNumber();
            if (GetReachableTargets().Contains(_dangerousTargets[targetNumber]))
            {
                return unit.Pos;
            }
            else
            {
                ActivePath = new SmartUnitPath(runtimeModel, unit.Pos, _dangerousTargets[targetNumber]);
                return ActivePath.GetNextStepFrom(unit.Pos);
            }
        }

        protected override List<Vector2Int> SelectTargets()
        {
            List<Vector2Int> result = new();
            List<Vector2Int> allTargets = new List<Vector2Int>(GetAllTargets());
            allTargets.Remove(GetEnemyBase());
            _dangerousTargets.Clear();

            if (allTargets.Any())
            {
                SortByDistanceToOwnBase(allTargets); // Сортируем цели
                _dangerousTargets.AddRange(allTargets.GetRange(0, Math.Min(allTargets.Count, MaxTargets)));
            }
            else
            {
                _dangerousTargets.Add(GetEnemyBase());
            }

            var targetNumber = GetTargetNumber();
            if (GetReachableTargets().Contains(_dangerousTargets[targetNumber]))
            {
                result.Add(_dangerousTargets[targetNumber]);
            }
            return result;
        }

        private int GetTargetNumber()
        {
            return Math.Min(_dangerousTargets.Count - 1, UnitID % MaxTargets);
        }

        protected Vector2Int GetEnemyBase()
        {
            return runtimeModel.RoMap.Bases[
                IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId];
        }

        public override void Update(float deltaTime, float time)
        {
            if (_overheated)
            {
                _cooldownTime += Time.deltaTime;
                float t = _cooldownTime / (OverheatCooldown / 10);
                _temperature = Mathf.Lerp(OverheatTemperature, 0, t);
                if (t >= 1)
                {
                    _cooldownTime = 0;
                    _overheated = false;
                }
            }
        }

        private int GetTemperature()
        {
            return _overheated ? (int)OverheatTemperature : (int)_temperature;
        }

        private void IncreaseTemperature()
        {
            _temperature += 1f;
            if (_temperature >= OverheatTemperature)
            {
                _overheated = true;
            }
        }
    }
}
