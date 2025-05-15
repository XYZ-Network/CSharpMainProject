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
        private const int MaxTargetsToConsider = 3; // B. Константа для максимума целей

        private static int _unitCounter = 0; // A. Статический счетчик юнитов
        private readonly int _unitNumber; // A. Номер текущего юнита

        private float _temperature = 0f;
        private float _cooldownTime = 0f;
        private bool _overheated;
        private List<Vector2Int> _targetsToMove = new List<Vector2Int>();

        public SecondUnitBrain()
        {
            // A. Инициализация номера юнита при создании
            _unitNumber = _unitCounter++;
        }

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
            if (_targetsToMove.Count == 0)
                return unit.Pos;

            return unit.Pos.CalcNextStepTowards(_targetsToMove[0]);
        }

        protected override List<Vector2Int> SelectTargets()
        {
            List<Vector2Int> result = new List<Vector2Int>();
            _targetsToMove.Clear();

            // B. Получаем и сортируем цели по расстоянию к базе
            List<Vector2Int> allTargets = GetAllTargets().ToList();
            SortByDistanceToOwnBase(allTargets);

            if (allTargets.Count > 0)
            {
                // B. Выбираем цель в зависимости от номера юнита
                int targetIndex = Mathf.Min(_unitNumber, allTargets.Count - 1, MaxTargetsToConsider - 1);
                Vector2Int selectedTarget = allTargets[targetIndex];

                if (IsTargetInRange(selectedTarget))
                {
                    result.Add(selectedTarget);
                }
                else
                {
                    _targetsToMove.Add(selectedTarget);
                }
            }
            else
            {
                // C. Если целей нет - атакуем базу противника
                int enemyBaseId = IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId;
                Vector2Int enemyBase = runtimeModel.RoMap.Bases[enemyBaseId];
                _targetsToMove.Add(enemyBase);
            }

            return result;
        }

        // Метод для сортировки целей по расстоянию к базе
        private void SortByDistanceToOwnBase(List<Vector2Int> targets)
        {
            targets.Sort((a, b) =>
            {
                float distA = DistanceToOwnBase(a);
                float distB = DistanceToOwnBase(b);
                return distA.CompareTo(distB);
            });
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