using Model;
using Model.Config;
using Model.Runtime.Projectiles;
using Model.Runtime.ReadOnly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Tactics;
using UnitBrains.Pathfinding;
using UnitBrains.Player;
using UnityEngine;
using Utilities;
using Baffs;
using static UnityEngine.GraphicsBuffer;
using Unit = Model.Runtime.Unit;

namespace UnitBrains
{
    public abstract class BaseUnitBrain
    {
        public virtual string TargetUnitName => string.Empty;
        public virtual bool IsPlayerUnitBrain => true;
        public virtual BaseUnitPath ActivePath => _activePath;
        
        protected Unit unit { get; private set; }

        protected DefaultPlayerUnitTactics tactic;
        protected IReadOnlyRuntimeModel runtimeModel => ServiceLocator.Get<IReadOnlyRuntimeModel>();
        protected private BaseUnitPath _activePath = null;

        public float AttackRange { get; set; }
        
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

            PositionWithDanger targetPos = tactic.GetPriorityTarget(IsPlayerUnitBrain);


            Vector2Int target;

            if (targetPos.Danger == 1)
            {
                if (IsTargetInRange(targetPos.Position))
                {
                    target = unit.Pos;
                }
                else
                {
                    target = targetPos.Position;

                }
            }
            else if (HasTargetsInRange())
            {
                target =  unit.Pos;
            }
            else
            {
                target = targetPos.Position;
            }

            _activePath = new UnitPath(runtimeModel, unit.Pos, target);
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
            this.AttackRange = unit.Config.AttackRange;
        }

        public Unit GetUnit() { return unit; }

        public void SetTactic(DefaultPlayerUnitTactics tactic)
        {
            this.tactic = tactic;
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
            PositionWithDanger targetPos = tactic.GetPriorityTarget(IsPlayerUnitBrain);
            List<Vector2Int> result = new List<Vector2Int>();
            if (targetPos.Danger == 1)
            {
                if (IsTargetInRange(targetPos.Position))
                {
                    result.Add(targetPos.Position);
                    return result;
                }
                return result;
            }
            else if (HasTargetsInRange())
            {
                if (GetReachableTargetsWithoutBases().OrderBy(x => GetUnitAt(x).Health).FirstOrDefault() == null)
                {
                    result.Add(runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId]);
                }
                else
                {
                    result.Add(GetReachableTargetsWithoutBases().OrderBy(x => GetUnitAt(x).Health).FirstOrDefault());
                }
                return result;
            }

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
            var attackRangeSqr = AttackRange * AttackRange;
            foreach (var possibleTarget in GetAllTargets())
            {
                var diff = possibleTarget - unit.Pos;
                if (diff.sqrMagnitude <= attackRangeSqr)
                    return true;
            }

            return false;
        }
        protected bool HasAlliesInRange()
        {
            var attackRangeSqr = AttackRange * AttackRange;
            foreach (var possibleAlly in GetAllAllies())
            {
                var diff = possibleAlly - unit.Pos;
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
                .Append(runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId]);
        }

        protected IEnumerable<Vector2Int> GetAllTargetsWithoutBases()
        {
            return runtimeModel.RoUnits
                .Where(u => u.Config.IsPlayerUnit != IsPlayerUnitBrain)
                .Select(u => u.Pos);
        }
        protected IEnumerable<Vector2Int> GetAllAllies()
        {
            return runtimeModel.RoUnits
                .Where(u => u.Config.IsPlayerUnit == IsPlayerUnitBrain && u.Pos != unit.Pos)
                .Select(u => u.Pos);
        }
        protected bool IsTargetInRange(Vector2Int targetPos)
        {
            var attackRangeSqr = AttackRange * AttackRange;
            var diff = targetPos - unit.Pos;
            return diff.sqrMagnitude <= attackRangeSqr;
        }

        protected List<Vector2Int> GetReachableTargets()
        {
            var result = new List<Vector2Int>();
            var attackRangeSqr = AttackRange * AttackRange;
            foreach (var possibleTarget in GetAllTargets())
            {
                if (!IsTargetInRange(possibleTarget))
                    continue;
                
                result.Add(possibleTarget);
            }

            return result;
        }

        protected List<Vector2Int> GetReachableTargetsWithoutBases()
        {
            var result = new List<Vector2Int>();
            var attackRangeSqr = AttackRange * AttackRange;
            foreach (var possibleTarget in GetAllTargetsWithoutBases())
            {
                if (!IsTargetInRange(possibleTarget))
                    continue;

                result.Add(possibleTarget);
            }

            return result;
        }

        protected List<Vector2Int> GetReachableAllies()
        {
            var result = new List<Vector2Int>();
            var attackRangeSqr = AttackRange * AttackRange;
            foreach (var possibleAlly in GetAllAllies())
            {
                if (!IsTargetInRange(possibleAlly))
                    continue;

                result.Add(possibleAlly);
            }

            return result;
        }

        public bool TryApplyEffect(BaseStatusEffect status)
        {
            try
            {
                status.Effect(this);
                Debug.Log("buff!!!");
                status.StartStatus(this);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.ToString());
                return false;
            }
            return true;
        }
    }
}