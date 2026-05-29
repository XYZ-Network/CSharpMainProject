using System.Collections.Generic;
using System.Linq;
using Model;
using Model.Runtime.Projectiles;
using Model.Runtime.ReadOnly;
using UnitBrains.Pathfinding;
using UnityEngine;
using Utilities;
using Unit = Model.Runtime.Unit;

namespace UnitBrains
{
    public abstract class BaseUnitBrain
    {
        public virtual string TargetUnitName => string.Empty;
        public virtual bool IsPlayerUnitBrain => true;
        public virtual BaseUnitPath ActivePath => _activePath;

        protected Unit unit { get; private set; }
        protected IReadOnlyRuntimeModel runtimeModel => ServiceLocator.Get<IReadOnlyRuntimeModel>();

        private BaseUnitPath _activePath;
        private PlayerCoordinator _coordinator;

        private readonly Vector2[] _projectileShifts =
        {
            new Vector2(0f, 0f),
            new Vector2(0.15f, 0f),
            new Vector2(0f, 0.15f),
            new Vector2(0.15f, 0.15f),
            new Vector2(0.15f, -0.15f),
            new Vector2(-0.15f, 0.15f),
            new Vector2(-0.15f, -0.15f),
        };

        public void SetUnit(Unit unit)
        {
            this.unit = unit;
        }

        public void SetCoordinator(PlayerCoordinator coordinator)
        {
            _coordinator = coordinator;
        }

        public virtual void Update(float deltaTime, float time)
        {
        }

        public virtual Vector2Int GetNextStep()
        {
            if (HasTargetsInRange())
                return unit.Pos;

            var target = runtimeModel.RoMap.Bases[
                IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId];

            if (IsPlayerUnitBrain)
                target = GetCoordinatorPointOrDefault(target);

            target = ResolveMoveTarget(target);

            _activePath = new AStarUnitPath(runtimeModel, unit.Pos, target);
            return _activePath.GetNextStepFrom(unit.Pos);
        }

        public List<BaseProjectile> GetProjectiles()
        {
            List<BaseProjectile> result = new();

            foreach (var target in SelectTargets())
                GenerateProjectiles(target, result);

            for (int i = 0; i < result.Count; i++)
            {
                var proj = result[i];
                proj.AddStartShift(_projectileShifts[i % _projectileShifts.Length]);
            }

            return result;
        }

        protected virtual void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            AddProjectileToList(CreateProjectile(forTarget), intoList);
        }

        protected virtual List<Vector2Int> SelectTargets()
        {
            if (TryGetCoordinatorTarget(out Vector2Int recommendedTarget))
                return new List<Vector2Int> { recommendedTarget };

            var result = GetReachableTargets();
            while (result.Count > 1)
                result.RemoveAt(result.Count - 1);

            return result;
        }

        protected BaseProjectile CreateProjectile(Vector2Int target) =>
            BaseProjectile.Create(unit.Config.ProjectileType, unit, unit.Pos, target, unit.Config.Damage);

        protected void AddProjectileToList(BaseProjectile projectile, List<BaseProjectile> list) =>
            list.Add(projectile);

        protected IReadOnlyUnit GetUnitAt(Vector2Int pos) =>
            runtimeModel.RoUnits.FirstOrDefault(u => u.Pos == pos);

        protected List<IReadOnlyUnit> GetUnitsInRadius(float radius, bool enemies)
        {
            var units = new List<IReadOnlyUnit>();
            var pos = unit.Pos;
            var distanceSqr = radius * radius;

            foreach (var otherUnit in runtimeModel.RoUnits)
            {
                if (otherUnit == unit)
                    continue;

                bool sameSide = otherUnit.Config.IsPlayerUnit == unit.Config.IsPlayerUnit;

                if (enemies == sameSide)
                    continue;

                var diff = otherUnit.Pos - pos;
                if (diff.sqrMagnitude <= distanceSqr)
                    units.Add(otherUnit);
            }

            return units;
        }

        protected bool HasTargetsInRange()
        {
            var attackRangeSqr = unit.CurrentAttackRange * unit.CurrentAttackRange;

            foreach (var possibleTarget in GetAllTargets())
            {
                var diff = possibleTarget - unit.Pos;
                if (diff.sqrMagnitude <= attackRangeSqr)
                    return true;
            }

            return false;
        }

        protected IEnumerable<IReadOnlyUnit> GetAllEnemyUnits()
        {
            return runtimeModel.RoUnits
                .Where(u => u.Config.IsPlayerUnit != IsPlayerUnitBrain);
        }

        protected IEnumerable<Vector2Int> GetAllTargets()
        {
            return runtimeModel.RoUnits
                .Where(u => u.Config.IsPlayerUnit != IsPlayerUnitBrain)
                .Select(u => u.Pos)
                .Append(runtimeModel.RoMap.Bases[
                    IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId]);
        }

        protected bool IsTargetInRange(Vector2Int targetPos)
        {
            var attackRangeSqr = unit.CurrentAttackRange * unit.CurrentAttackRange;
            var diff = targetPos - unit.Pos;
            return diff.sqrMagnitude <= attackRangeSqr;
        }

        protected List<Vector2Int> GetReachableTargets()
        {
            var result = new List<Vector2Int>();

            foreach (var possibleTarget in GetAllTargets())
            {
                if (!IsTargetInRange(possibleTarget))
                    continue;

                result.Add(possibleTarget);
            }

            return result;
        }

        protected bool TryGetCoordinatorTarget(out Vector2Int target)
        {
            target = unit.Pos;

            if (!IsPlayerUnitBrain)
                return false;

            if (_coordinator == null)
                return false;

            _coordinator.Recalculate();

            if (!_coordinator.HasRecommendedTarget)
                return false;

            float doubleAttackRange = unit.CurrentAttackRange * 2f;
            var diff = _coordinator.RecommendedTarget - unit.Pos;

            if (diff.sqrMagnitude > doubleAttackRange * doubleAttackRange)
                return false;

            target = _coordinator.RecommendedTarget;
            return true;
        }

        protected Vector2Int GetCoordinatorPointOrDefault(Vector2Int fallback)
        {
            if (!IsPlayerUnitBrain)
                return fallback;

            if (_coordinator == null)
                return fallback;

            _coordinator.Recalculate();

            if (!_coordinator.HasRecommendedTarget)
                return fallback;

            return GetDistributedCoordinatorPoint(_coordinator.RecommendedPoint, fallback);
        }

        protected Vector2Int GetDistributedCoordinatorPoint(Vector2Int center, Vector2Int fallback)
        {
            var playerUnits = runtimeModel.RoUnits
                .Where(u => u.Config.IsPlayerUnit)
                .OrderBy(u => u.Pos.x)
                .ThenBy(u => u.Pos.y)
                .ToList();

            int unitIndex = playerUnits.FindIndex(u => u == unit);
            if (unitIndex < 0)
                return fallback;

            List<Vector2Int> candidates = new()
            {
                center,
                center + new Vector2Int(1, 0),
                center + new Vector2Int(-1, 0),
                center + new Vector2Int(0, 1),
                center + new Vector2Int(0, -1),
                center + new Vector2Int(1, 1),
                center + new Vector2Int(1, -1),
                center + new Vector2Int(-1, 1),
                center + new Vector2Int(-1, -1),
                center + new Vector2Int(2, 0),
                center + new Vector2Int(-2, 0),
                center + new Vector2Int(0, 2),
                center + new Vector2Int(0, -2),
            };

            List<Vector2Int> available = new();

            foreach (var cell in candidates)
            {
                if (runtimeModel.RoMap[cell])
                    continue;

                if (runtimeModel.RoUnits.Any(u => u != unit && u.Pos == cell))
                    continue;

                available.Add(cell);
            }

            if (available.Count == 0)
                return fallback;

            return available[unitIndex % available.Count];
        }

        private Vector2Int ResolveMoveTarget(Vector2Int rawTarget)
        {
            if (!runtimeModel.RoMap[rawTarget])
                return rawTarget;

            Vector2Int[] around =
            {
                rawTarget + Vector2Int.up,
                rawTarget + Vector2Int.right,
                rawTarget + Vector2Int.down,
                rawTarget + Vector2Int.left
            };

            Vector2Int best = unit.Pos;
            int bestDistance = int.MaxValue;
            bool found = false;

            foreach (var cell in around)
            {
                if (runtimeModel.RoMap[cell])
                    continue;

                if (runtimeModel.RoUnits.Any(u => u != unit && u.Pos == cell))
                    continue;

                int dist = Mathf.Abs(cell.x - unit.Pos.x) + Mathf.Abs(cell.y - unit.Pos.y);
                if (dist < bestDistance)
                {
                    bestDistance = dist;
                    best = cell;
                    found = true;
                }
            }

            return found ? best : rawTarget;
        }
    }
}