using Model.Runtime.Projectiles;
using System.Collections.Generic;
using System.Linq;
using UnitBrains.Pathfinding;
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

        private static int _unitCounter = 0;                         //  статический счётчик
        private int _unitNumber;                                     // номер текущего юнита
        private const int MaxTargetCount = 3;                        // максимум целей на выбор

        private bool _initialized = false;

        protected void Awake()
        {
            _unitNumber = _unitCounter++;
            Debug.Log($"[SecondUnitBrain] unitNumber = {_unitNumber}");
        }

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            if (_overheated)
            {
                UnityEngine.Debug.Log("overheated, cooling down...");
                return;
            }

            if (!IsInAttackRange(forTarget))
            {
                return;
            }

            IncreaseTemperature();

            int bulletCount = GetTemperature();

            for (int i = 0; i < bulletCount; i++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }
        }

        protected override List<Vector2Int> SelectTargets()
        {
            var rawTargets = GetAllTargets();
            Debug.Log($"[SelectTargets] Сырой список целей: {string.Join(", ", rawTargets)}");
            List<Vector2Int> allTargets = rawTargets.ToList();

            if (allTargets.Count == 0)
                return new List<Vector2Int>();

            allTargets.Sort((a, b) =>
                (a - unit.Pos).sqrMagnitude.CompareTo((b - unit.Pos).sqrMagnitude));

            int targetIndex = _unitNumber % MaxTargetCount;

            if (targetIndex >= allTargets.Count)
                targetIndex = 0;
            Debug.Log($"[SelectTargets] Юнит {_unitNumber} выбирает цель №{targetIndex} из {allTargets.Count}");

            Vector2Int target = allTargets[targetIndex];

            Debug.Log($"[SelectTargets] Юнит {_unitNumber} выбирает координаты цели: {target}");

            if (IsInAttackRange(target))
                return new List<Vector2Int> { target };

            return new List<Vector2Int>();
        }

        public override Vector2Int GetNextStep()
        {
            var targets = SelectTargets();

            if (targets.Count == 0)
                return base.GetNextStep();

            var target = targets[0];

            if (IsTargetInRange(target))
                return unit.Pos;

            return new DummyUnitPath(runtimeModel, unit.Pos, target).GetNextStepFrom(unit.Pos);
        }

        public override void Update(float deltaTime, float time)
        {
            if (!_initialized)
            {
                _unitNumber = _unitCounter++;
                Debug.Log($"[Init] Назначен номер юнита: {_unitNumber}");
                _initialized = true;
            }

            if (_overheated)
            {
                _cooldownTime += UnityEngine.Time.deltaTime;
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
                _overheated = true;
        }

        private bool IsInAttackRange(Vector2Int targetPos)
        {
            var attackRangeSqr = unit.Config.AttackRange * unit.Config.AttackRange;
            var diff = targetPos - unit.Pos;
            return diff.sqrMagnitude <= attackRangeSqr;
        }
    }
}