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

        // Цели, к которым нужно двигаться
        private readonly List<Vector2Int> _targetsToMove = new();

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            var projectile = CreateProjectile(forTarget);
            AddProjectileToList(projectile, intoList);
        }

        public override Vector2Int GetNextStep()
        {
            if (_targetsToMove.Count == 0)
                return unit.Pos;

            Vector2Int target = _targetsToMove[0];

            // Если цель уже в зоне атаки — стоим
            if (IsTargetInRange(target))
                return unit.Pos;

            // Иначе идём к цели
            return unit.Pos.CalcNextStepTowards(target);
        }

        protected override List<Vector2Int> SelectTargets()
        {
            List<Vector2Int> result = new();
            _targetsToMove.Clear();

            // Получаем все цели
            List<Vector2Int> allTargets = GetAllTargets().ToList();

            // База противника уже входит в GetAllTargets(),
            // но по заданию её надо атаковать только если врагов нет
            Vector2Int enemyBase = runtimeModel.RoMap.Bases[
                IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId
            ];

            allTargets.Remove(enemyBase);

            if (allTargets.Count > 0)
            {
                // Ищем самую опасную цель — ближайшую к нашей базе
                Vector2Int myBase = runtimeModel.RoMap.Bases[
                    IsPlayerUnitBrain ? RuntimeModel.PlayerId : RuntimeModel.BotPlayerId
                ];

                Vector2Int mostDangerous = allTargets[0];
                int minDist = Distance(myBase, mostDangerous);

                for (int i = 1; i < allTargets.Count; i++)
                {
                    int dist = Distance(myBase, allTargets[i]);

                    if (dist < minDist)
                    {
                        minDist = dist;
                        mostDangerous = allTargets[i];
                    }
                }

                _targetsToMove.Add(mostDangerous);

                if (IsTargetInRange(mostDangerous))
                    result.Add(mostDangerous);
            }
            else
            {
                // Если врагов нет — идём к базе противника
                _targetsToMove.Add(enemyBase);

                if (IsTargetInRange(enemyBase))
                    result.Add(enemyBase);
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
            if (_overheated)
                return (int)OverheatTemperature;

            return (int)_temperature;
        }

        private void IncreaseTemperature()
        {
            _temperature += 1f;

            if (_temperature >= OverheatTemperature)
                _overheated = true;
        }

        private int Distance(Vector2Int a, Vector2Int b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }
    }
}