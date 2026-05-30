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
        // ========== СИНГЛТОН-КООРДИНАТОР ==========
        private static BaseUnitBrain _coordinatorInstance;
        private static readonly object _lock = new object();

        public static BaseUnitBrain Coordinator
        {
            get
            {
                lock (_lock)
                {
                    if (_coordinatorInstance == null)
                    {
                        _coordinatorInstance = new CoordinatorBrain();
                    }
                    return _coordinatorInstance;
                }
            }
        }

        // Вспомогательный класс-координатор (наследник)
        private class CoordinatorBrain : BaseUnitBrain
        {
            public override bool IsPlayerUnitBrain => true;

            public override Vector2Int GetRecommendedTarget()
            {
                var model = ServiceLocator.Get<IReadOnlyRuntimeModel>();
                var enemies = model.RoUnits
                    .Where(u => u.Config.IsPlayerUnit == false)
                    .ToList();

                if (enemies.Count == 0)
                    return model.RoMap.Bases[RuntimeModel.BotPlayerId];

                return enemies.OrderBy(e => e.Health).First().Pos;
            }

            public override Vector2Int GetRecommendedPosition()
            {
                var model = ServiceLocator.Get<IReadOnlyRuntimeModel>();
                var myBase = model.RoMap.Bases[RuntimeModel.PlayerId];
                return new Vector2Int(myBase.x - 1, myBase.y);
            }
        }

        // Виртуальные методы координации
        public virtual Vector2Int GetRecommendedTarget()
        {
            // Базовая реализация (будет переопределена в CoordinatorBrain)
            var enemies = GetAllEnemyUnits().ToList();
            if (enemies.Count == 0)
                return runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId];
            return enemies.OrderBy(e => e.Health).First().Pos;
        }

        public virtual Vector2Int GetRecommendedPosition()
        {
            var myBase = runtimeModel.RoMap.Bases[RuntimeModel.PlayerId];
            return new Vector2Int(myBase.x - 1, myBase.y);
        }
        // ========== КОНЕЦ КООРДИНАЦИИ ==========

        public virtual string TargetUnitName => string.Empty;
        public virtual bool IsPlayerUnitBrain => true;
        public virtual BaseUnitPath ActivePath => _activePath;

        protected Unit unit { get; private set; }
        protected IReadOnlyRuntimeModel runtimeModel => ServiceLocator.Get<IReadOnlyRuntimeModel>();
        private BaseUnitPath _activePath = null;
        private Vector2Int _lastTarget = new Vector2Int(-999, -999);
        private int _repathCounter = 0;

        private readonly Vector2[] _projectileShifts = new Vector2[]
        {
            new (0f, 0f),
            new (0.15f, 0f),
            new (0f, 0.15f),
            new (0.15f, 0.15f),
            new (0.15f, -0.15f),
            new (-0.15f, 0.15f),
            new (-0.15f, -0.15f),
        };

        public virtual Vector2Int GetNextStep()
        {
            if (HasTargetsInRange())
                return unit.Pos;

            // Используем координатор
            var target = GetRecommendedTarget();

            if (_activePath == null || _lastTarget != target || _repathCounter++ > 30)
            {
                _lastTarget = target;
                _activePath = new DummyUnitPath(runtimeModel, unit.Pos, target);
                _repathCounter = 0;
            }

            var nextStep = _activePath.GetNextStepFrom(unit.Pos);

            if (nextStep == unit.Pos)
            {
                var diagonalStep = TryDiagonalStep(target);
                if (diagonalStep != unit.Pos)
                {
                    return diagonalStep;
                }
            }

            return nextStep;
        }

        private Vector2Int TryDiagonalStep(Vector2Int target)
        {
            var directions = new Vector2Int[]
            {
                new Vector2Int(1, 1), new Vector2Int(1, -1),
                new Vector2Int(-1, 1), new Vector2Int(-1, -1),
                new Vector2Int(0, 1), new Vector2Int(0, -1),
                new Vector2Int(1, 0), new Vector2Int(-1, 0),
            };

            var sorted = directions
                .OrderBy(dir => Vector2Int.Distance(unit.Pos + dir, target))
                .ToList();

            foreach (var dir in sorted)
            {
                var newPos = unit.Pos + dir;
                if (runtimeModel.IsTileWalkable(newPos))
                {
                    var unitAtPos = runtimeModel.RoUnits.FirstOrDefault(u => u.Pos == newPos);
                    if (unitAtPos == null || unitAtPos.Config.IsPlayerUnit != IsPlayerUnitBrain)
                    {
                        return newPos;
                    }
                }
            }

            return unit.Pos;
        }

        public List<BaseProjectile> GetProjectiles()
        {
            List<BaseProjectile> result = new();
            foreach (var target in SelectTargets())
            {
                GenerateProjectiles(target, result);
            }

            for (int i = 0; i < result.Count; i++)
            {
                var proj = result[i];
                proj.AddStartShift(_projectileShifts[i % _projectileShifts.Length]);
            }

            return result;
        }

        public void SetUnit(Unit unit)
        {
            this.unit = unit;
        }

        public virtual void Update(float deltaTime, float time)
        {
        }

        protected virtual void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            AddProjectileToList(CreateProjectile(forTarget), intoList);
        }

        protected virtual List<Vector2Int> SelectTargets()
        {
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

                if (enemies != (otherUnit.Config.IsPlayerUnit == unit.Config.IsPlayerUnit))
                    continue;

                var otherPos = otherUnit.Pos;
                var diff = otherPos - pos;
                var distance = diff.sqrMagnitude;
                if (distance <= distanceSqr)
                    units.Add(otherUnit);
            }

            return units;
        }

        protected bool HasTargetsInRange()
        {
            var attackRangeSqr = unit.Config.AttackRange * unit.Config.AttackRange;
            foreach (var possibleTarget in GetAllTargets())
            {
                var diff = possibleTarget - unit.Pos;
                if (diff.sqrMagnitude < attackRangeSqr)
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
                .Append(runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId]);
        }

        protected bool IsTargetInRange(Vector2Int targetPos)
        {
            var attackRangeSqr = unit.Config.AttackRange * unit.Config.AttackRange;
            var diff = targetPos - unit.Pos;
            return diff.sqrMagnitude <= attackRangeSqr;
        }

        protected List<Vector2Int> GetReachableTargets()
        {
            var result = new List<Vector2Int>();
            var attackRangeSqr = unit.Config.AttackRange * unit.Config.AttackRange;
            foreach (var possibleTarget in GetAllTargets())
            {
                if (!IsTargetInRange(possibleTarget))
                    continue;

                result.Add(possibleTarget);
            }

            return result;
        }
    }
}