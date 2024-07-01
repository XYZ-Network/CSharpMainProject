using Model;
using Model.Runtime.Projectiles;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnitBrains.Player
{
    public class SecondUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Cobra Commando";
        private const float OverheatTemperature = 3f;
        private const float OverheatCooldown = 2f;
        private float _temperature = 0f;
        private float _cooldownTime = 0f;
        private bool _overheated;

        private const int MaxTargetsCount = 3;
        private static int _unitNumberCounter = 0;
        private int _unitNumber;

        private List<Vector2Int> _unreacheabletargets = new List<Vector2Int>();

        public SecondUnitBrain()
        {
            _unitNumber = _unitNumberCounter++;
        }

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;

            var currentTemperature = GetTemperature();

            if (overheatTemperature == currentTemperature)
            {
                return;
            }

            var shotsCount = currentTemperature + 1;

            for (var i = 0; i < shotsCount; i++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }

            IncreaseTemperature();
        }

        public override Vector2Int GetNextStep()
        {
            Vector2Int position = Vector2Int.zero;

            var target = _unreacheabletargets.FirstOrDefault();

            if (target != null)
            {
                if (IsTargetInRange(target))
                {
                    position = unit.Pos;
                }
                else
                {
                    position = base.GetNextStep();
                }
            }

            return position;
        }

        protected override List<Vector2Int> SelectTargets()
        {
            var result = new List<Vector2Int>(GetAllTargets());

            if (result.Count == 0)
            {
                if (IsPlayerUnitBrain)
                {
                    result.Add(runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId]);
                }
                else
                {
                    result.Add(runtimeModel.RoMap.Bases[RuntimeModel.PlayerId]);
                }
            }

            SortByDistanceToOwnBase(result);

            _unreacheabletargets.Clear();

            var currentCheckIndex = _unitNumber % Mathf.Min(MaxTargetsCount, result.Count);
            var currentCheckTarget = result[currentCheckIndex];

            result.Clear();

            if (IsTargetInRange(currentCheckTarget))
            {
                result.Add(currentCheckTarget);
            }
            else
            {
                _unreacheabletargets.Add(currentCheckTarget);
            }

            return result;
        }
        private void SortByDistanceToOwnBase(List<Vector2Int> list)
        {
            list.Sort(CompareByDistanceToOwnBase);
        }

        private int CompareByDistanceToOwnBase(Vector2Int a, Vector2Int b)
        {
            var distanceA = DistanceToOwnBase(a);
            var distanceB = DistanceToOwnBase(b);
            return distanceA.CompareTo(distanceB);
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
            if (_overheated) return (int)OverheatTemperature;
            else return (int)_temperature;
        }

        private void IncreaseTemperature()
        {
            _temperature += 1f;
            if (_temperature >= OverheatTemperature) _overheated = true;
        }
    }
}
