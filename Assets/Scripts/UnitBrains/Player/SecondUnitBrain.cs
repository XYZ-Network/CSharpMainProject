using Model;
using Model.Runtime.Projectiles;
using System;
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
        private List<Vector2Int> _targetsToMove = new();

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            if (GetTemperature() >= OverheatTemperature)
                return;

            IncreaseTemperature();

            for (int i = 0; i < GetTemperature(); i++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }
        }

        protected override List<Vector2Int> SelectTargets()
        {
            var result = new List<Vector2Int>();
            var allTargets = GetAllTargets().ToList();
            var playerBase = runtimeModel.RoMap.Bases[RuntimeModel.PlayerId];
            var enemyBase = runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId];

            var enemyUnits = allTargets
                .Where(t => t != playerBase && t == enemyBase || t != playerBase && t != null)
                .ToList();
            
            bool hasEnemyUnits = enemyUnits.Any();

            if (hasEnemyUnits)
            {
                var selectedEnemy = enemyUnits.First();

                if (IsTargetInRange(selectedEnemy))
                {
                    _targetsToMove.Clear();
                    result.Add(selectedEnemy);
                }
                else
                {
                    _targetsToMove = new List<Vector2Int> { selectedEnemy };
                }
            }

            if (!hasEnemyUnits || result.Count == 0)
            {
                if (IsTargetInRange(enemyBase))
                {
                    result.Add(enemyBase);
                }
                else
                {
                    if (_targetsToMove.Count == 0)
                        _targetsToMove = new List<Vector2Int> { enemyBase };
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