using System.Collections.Generic;
using System.Linq;
using Model;
using Model.Runtime.Projectiles;
using Model.Runtime.ReadOnly;
using UnitBrains.Pathfinding;
using UnityEngine;
using Utilities;
using Unit = Model.Runtime.Unit;

//основные мозги юнитов
namespace UnitBrains
{
    public abstract class BaseUnitBrain
    {
        public virtual string TargetUnitName => string.Empty; // имя юнита
        public virtual bool IsPlayerUnitBrain => true; // принадлежит ли юнит игроку
        public virtual BaseUnitPath ActivePath => _activePath; // активный путь по которому сейчас идёт юнит, это свойство которое возращает значение ниже - private BaseUnitPath _activePath = null
        protected Unit unit { get; private set; } // ссылка на самого юнита, которому пренадлежить unit
        protected IReadOnlyRuntimeModel runtimeModel => ServiceLocator.Get<IReadOnlyRuntimeModel>(); // в runtimeModel находятся все данные по текущей игровой сессии
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

        public virtual Vector2Int GetNextStep() // возвращает следующий шаг
        {
            if (HasTargetsInRange()) // если цели в зоне досягаемости, то ходить никуда не нужно
                return unit.Pos;

            var target = runtimeModel.RoMap.Bases[
                IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId];

            _activePath = new AStarPathFinding(runtimeModel, unit.Pos, target); // активный путь
            //_activePath = new DummyUnitPath(runtimeModel, unit.Pos, target); // активный путь
            return _activePath.GetNextStepFrom(unit.Pos); 
        }

        public List<BaseProjectile> GetProjectiles()  // возвращает все тайлы
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

        public virtual void Update(float deltaTime, float time) // можно переопределить для создания необычной логики
        {
        }

        protected virtual void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            AddProjectileToList(CreateProjectile(forTarget), intoList);
        }

        protected virtual List<Vector2Int> SelectTargets() // возвращает список целей
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