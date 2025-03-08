using System;
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
        private BaseUnitPath _activePath = null;
        
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
        
        // модификаторы
        public float DefaultAttackRange => unit.Config.AttackRange;
        public float DefaultShootIndex => 1;
        
        private float _attackRange;
        private float _doubleShootIndex;
        
        public float AttackRange // Done
        {
            get => _attackRange != 0 ? _attackRange: DefaultAttackRange;
            set => _attackRange = value;
        }
        public float DoubleShootIndex // Done
        {
            get => _doubleShootIndex != 0 ? _doubleShootIndex : DefaultShootIndex;
            set => _doubleShootIndex = value;
        }
        
        public virtual Vector2Int GetNextStep()
        {
            if (HasTargetsInRange())
                return unit.Pos;

            var target = runtimeModel.RoMap.Bases[
                IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId];

            _activePath = new DummyUnitPath(runtimeModel, unit.Pos, target);
            return _activePath.GetNextStepFrom(unit.Pos);
        }

        public List<BaseProjectile> GetProjectiles()
        {
            List<BaseProjectile> result = new ();
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

        protected virtual void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList) // тут будет индекс двойного выстрела
        {
            for (int i = 0; i < DoubleShootIndex ; i++) // DoubleShootIndex - индекс пуль
            {
                AddProjectileToList(CreateProjectile(forTarget), intoList);
            }
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

        protected bool HasTargetsInRange() // радиус атаки
        {
            //var attackRangeSqr = unit.Config.AttackRange * unit.Config.AttackRange;
            var attackRangeSqr = AttackRange * AttackRange;
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

        protected bool IsTargetInRange(Vector2Int targetPos) // тут будем увеличивать радиус атаки
        {
            //var attackRangeSqr = unit.Config.AttackRange * unit.Config.AttackRange;
            var attackRangeSqr = AttackRange * AttackRange;
            var diff = targetPos - unit.Pos;
            return diff.sqrMagnitude <= attackRangeSqr;
        }

        protected List<Vector2Int> GetReachableTargets() 
        {
            var result = new List<Vector2Int>();
            //var attackRangeSqr = unit.Config.AttackRange * unit.Config.AttackRange;
            var attackRangeSqr = AttackRange * AttackRange;
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