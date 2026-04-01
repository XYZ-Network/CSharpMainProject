using System.Collections.Generic;
using System.Linq;
using Model.Runtime.Projectiles;
using UnityEngine;
using Model.Runtime;


namespace UnitBrains.Player
{
    public class SecondUnitBrain : DefaultPlayerUnitBrain
    {
        // ====== A. Статический счетчик ======
        private static int _unitCounter = 0;
        private int _unitNumber;

        private const int MaxSmartTargets = 3;

        private List<Vector2Int> _targetsToMove = new List<Vector2Int>();

        public override string TargetUnitName => "Cobra Commando";

        private const float OverheatTemperature = 3f;
        private const float OverheatCooldown = 2f;

        private float _temperature = 0f;
        private float _cooldownTime = 0f;
        private bool _overheated;

        // Small per-unit offsets to prevent all units stacking on the same tile
        private static readonly Vector2Int[] _unitOffsets =
        {
            new(0, 0), new(1, 0), new(-1, 0),
            new(0, 1), new(0, -1), new(1, 1), new(-1, -1),
        };

        // ====== Инициализация номера юнита ======
        public override void SetUnit(Unit unit)
        {
            base.SetUnit(unit);

            _unitNumber = _unitCounter;
            _unitCounter++;
        }

        // ====== Выбор целей ======
        protected override List<Vector2Int> SelectTargets()
        {
            var result = new List<Vector2Int>();
            _targetsToMove.Clear();

            // Use the coordinator's recommended target if it's within 2x attack range
            var coordinator = UnitCoordinator.Instance;
            var coordinatorTarget = coordinator.GetRecommendedTarget();

            if (coordinatorTarget != null)
            {
                var distToTarget = Vector2Int.Distance(unit.Pos, coordinatorTarget.Pos);
                if (distToTarget <= unit.Config.AttackRange * 2f)
                {
                    _targetsToMove.Add(coordinatorTarget.Pos);
                    if (IsTargetInRange(coordinatorTarget.Pos))
                        result.Add(coordinatorTarget.Pos);
                    return result;
                }
            }

            // Fall back to individual target selection
            var allTargets = GetAllTargets().ToList();

            if (allTargets.Count == 0)
            {
                var enemyBase = runtimeModel.RoMap.Bases.First();
                allTargets.Add(enemyBase);
            }

            // Сортируем по расстоянию до своей базы
            SortByDistanceToOwnBase(allTargets);

            // Определяем индекс цели
            int smartIndex = _unitNumber % MaxSmartTargets;

            if (smartIndex >= allTargets.Count)
                smartIndex = 0;

            var selectedTarget = allTargets[smartIndex];

            _targetsToMove.Add(selectedTarget);

            if (IsTargetInRange(selectedTarget))
                result.Add(selectedTarget);

            return result;
        }

        // ====== Движение ======
        public override Vector2Int GetNextStep()
        {
            // Stay put if there's a target already in attack range
            if (HasTargetsInRange())
                return unit.Pos;

            // Move toward the coordinator's recommended position, offset per unit to avoid stacking
            var coordinator = UnitCoordinator.Instance;
            var offset = _unitOffsets[_unitNumber % _unitOffsets.Length];
            var targetPos = coordinator.GetRecommendedPosition() + offset;

            var dir = targetPos - unit.Pos;
            if (dir == Vector2Int.zero)
                return unit.Pos;

            return new Vector2Int(
                unit.Pos.x + Mathf.Clamp(dir.x, -1, 1),
                unit.Pos.y + Mathf.Clamp(dir.y, -1, 1)
            );
        }

        // ====== Стрельба ======
        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            if (GetTemperature() >= OverheatTemperature)
                return;

            IncreaseTemperature();

            int shotsCount = Mathf.Max(1, (int)_temperature);

            for (int i = 0; i < shotsCount; i++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }
        }

        // ====== Обновление ======
        public override void Update(float deltaTime, float time)
        {
            if (_overheated)
            {
                _cooldownTime += deltaTime;

                float t = _cooldownTime / OverheatCooldown;

                _temperature = Mathf.Lerp(OverheatTemperature, 0f, t);

                if (t >= 1f)
                {
                    _cooldownTime = 0f;
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
    }
}