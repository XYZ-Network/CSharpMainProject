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
        // Статический счётчик для нумерации юнитов
        private static int _unitCounter = 0;
        private readonly int _unitNumber;

        public override string TargetUnitName => "Cobra Commando";
        private const float OverheatTemperature = 3f;
        private const float OverheatCooldown = 2f;
        private const int _smartMaxTargets = 3;
        private float _temperature = 0f;
        private float _cooldownTime = 0f;
        private bool _overheated;
        private Vector2Int _targetToAttack;

        public SecondUnitBrain()
        {
            _unitNumber = _unitCounter;
            _unitCounter++;
        }

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            ///////////////////////////////////////
            // Homework 1.3 (1st block, 3rd module)
            ///////////////////////////////////////
            float temperature = GetTemperature();
            if (temperature >= overheatTemperature)
            {
                return;
            }

            for (int i = 0; i <= temperature; i++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }

            IncreaseTemperature();
            ///////////////////////////////////////
        }

        public override Vector2Int GetNextStep()
        {
            ///////////////////////////////////////
            // Homework 1.4 (1st block, 4rd module)
            ///////////////////////////////////////
            if (_targetToAttack == default)
                return unit.Pos;

            if (GetReachableTargets().Contains(_targetToAttack))
                return unit.Pos;
            else
                return unit.Pos.CalcNextStepTowards(_targetToAttack);
        }

        protected override List<Vector2Int> SelectTargets()
        {
            var result = new List<Vector2Int>();
            _targetToAttack = default;

            ///////////////////////////////////////
            // Homework 1.4 (1st block, 4rd module)
            ///////////////////////////////////////
            var allTargets = GetAllTargets().ToList();

            if (allTargets.Any())
            {
                var sortedTargets = allTargets.OrderBy(DistanceToOwnBase).ToList();
                int targetIndex = _unitNumber % _smartMaxTargets;
                _targetToAttack = sortedTargets.Count > targetIndex ? sortedTargets[targetIndex] : sortedTargets.Last();

                if (IsTargetInRange(_targetToAttack))
                {
                    result.Add(_targetToAttack);
                }
            }
            else
            {
                var enemyBase = runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId];
                _targetToAttack = enemyBase;
                if (IsTargetInRange(enemyBase))
                {
                    result.Add(enemyBase);
                }
            }

            return result;
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
