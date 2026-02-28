using Model;
using Model.Runtime.Projectiles;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities;

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

        private static int UnitCounter = 0;
        private int _unitNumber;
        private const int MaxTargetsCount = 3;

        private Vector2Int? _movementTarget = null;

        public SecondUnitBrain()
        {
            _unitNumber = UnitCounter++;
        }

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            if (!IsTargetInRange(forTarget)) return;
            if (GetTemperature() >= OverheatTemperature) return;

            for (int i = 0; i <= GetTemperature(); i++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }

            // Двойной выстрел — если бафф активен, дублируем все снаряды
            if (unit.DoubleShot)
            {
                var extra = CreateProjectile(forTarget);
                AddProjectileToList(extra, intoList);
            }

            IncreaseTemperature();
        }

        public override Vector2Int GetNextStep()
        {
            if (_movementTarget == null || IsTargetInRange(_movementTarget.Value))
                return unit.Pos;

            return unit.Pos.CalcNextStepTowards(_movementTarget.Value);
        }

        
        private float Distance(Vector2Int a, Vector2Int b)
        {
            return Vector2Int.Distance(a, b);
        }

        Vector2Int ClosestUnreachableTarget;

        protected override List<Vector2Int> SelectTargets()
        {
            _movementTarget = null;
            var result = new List<Vector2Int>();
            var allTargets = GetAllTargets().ToList();

            if (!allTargets.Any())
            {
                int enemyBaseId = IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId;
                allTargets.Add(runtimeModel.RoMap.Bases[enemyBaseId]);
            }

            SortByDistanceToOwnBase(allTargets);

            int targetIndex = _unitNumber % MaxTargetsCount;

            if (targetIndex >= allTargets.Count)
                targetIndex = 0;

            var selectedTarget = allTargets[targetIndex];

            if (IsTargetInRange(selectedTarget))
                result.Add(selectedTarget);
            else
                _movementTarget = selectedTarget;

            return result;
        }

        private new void SortByDistanceToOwnBase(List<Vector2Int> targets)
        {
            var ownBasePos = runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.PlayerId : RuntimeModel.BotPlayerId];
            targets.Sort((a, b) => Vector2Int.Distance(a, ownBasePos).CompareTo(Vector2Int.Distance(b, ownBasePos)));
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