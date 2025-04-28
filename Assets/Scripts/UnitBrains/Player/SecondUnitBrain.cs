using System.Collections.Generic;
using System.Linq;
using Model;
using Model.Runtime.Projectiles;
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
        private List<Vector2Int> _targetsToMove = new List<Vector2Int>(); // Поле для целей вне зоны досягаемости

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            if (GetTemperature() >= overheatTemperature)
            {
                return;
            }

            IncreaseTemperature();

            for (int i = 0; i < GetTemperature(); i++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }
        }

        public override Vector2Int GetNextStep()
        {
            // Если нет целей для движения - остаемся на месте
            if (_targetsToMove.Count == 0)
                return unit.Pos;

            // Получаем следующую позицию к ближайшей цели
            return unit.Pos.CalcNextStepTowards(_targetsToMove[0]);
        }

        protected override List<Vector2Int> SelectTargets()
        {
            List<Vector2Int> result = new List<Vector2Int>();
            _targetsToMove.Clear();

            // A. Получаем все цели, а не только достижимые
            List<Vector2Int> allTargets = GetAllTargets().ToList();


            // B. Если есть цели
            if (allTargets.Count > 0)
            {
                // Находим ближайшую к базе цель
                Vector2Int mostDangerousTarget = allTargets[0];
                float minDistance = DistanceToOwnBase(mostDangerousTarget);

                foreach (Vector2Int target in allTargets)
                {
                    float distance = DistanceToOwnBase(target);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        mostDangerousTarget = target;
                    }
                }

                // Проверяем, достижима ли цель
                if (IsTargetInRange(mostDangerousTarget))
                {
                    result.Add(mostDangerousTarget);
                }
                else
                {
                    _targetsToMove.Add(mostDangerousTarget);
                }
            }
            // C. Если целей нет - атакуем базу противника
            else
            {
                int enemyBaseId = IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId;
                Vector2Int enemyBase = runtimeModel.RoMap.Bases[enemyBaseId];
                _targetsToMove.Add(enemyBase);
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