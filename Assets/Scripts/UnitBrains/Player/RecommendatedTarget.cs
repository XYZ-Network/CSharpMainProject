using Codice.Client.BaseCommands.WkStatus.Printers;
using Model;
using Model.Runtime.ReadOnly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using UnityEngine;

namespace Assets.Scripts.UnitBrains.Player
{
    public class RecommendatedTarget
    {
        //public RecommendatedTarget Instance
        //{
        //    get
        //    {
        //        if (_instance is null)
        //            _instance = new RecommendatedTarget();

        //        return _instance;
        //    }
        //}

        private static RecommendatedTarget _instance;

        private IReadOnlyRuntimeModel _runtimeModel;
        private TimeUtil _timeUtil;

        public IReadOnlyUnit RecommendationTarget { get; private set; }
        public Vector2Int RecommendationPosition { get; private set; }

        public Vector2Int PlayerBase => _runtimeModel.RoMap.Bases[RuntimeModel.PlayerId];
        public Vector2Int EnemyBase => _runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId];
        public RecommendatedTarget(IReadOnlyRuntimeModel runtimeModel, TimeUtil timeUtil)
        {
            //_runtimeModel = ServiceLocator.Get<IReadOnlyRuntimeModel>();
            //_timeUtil = ServiceLocator.Get<TimeUtil>();
            _runtimeModel = runtimeModel;

            _timeUtil.AddUpdateAction(Update);
        }

        private void Update(float time)
        {
            if (_runtimeModel == null) 
                return;
            RecommendationTarget = GetTargetRecommendatedUnit();
            RecommendationPosition = GetTargetRecommendatedPosition();
        }

        ~RecommendatedTarget()
        {
            _timeUtil.RemoveUpdateAction(Update);
        }

        private IReadOnlyUnit GetTargetRecommendatedUnit()
        {
            List<IReadOnlyUnit> units = GetUnits();

            if (units.Count == 0)
                return null;

            if (units.Any(unit => GetDistanceToPlayerBase(unit) <= GetDistanceToEnemyBase(unit)))
            {
                units.Sort(CompareDistanceToPlayerBase);
            }
            else
            {
                units.Sort(CompareHealthEnemy);
            }
            return units[0];
        }

        private Vector2Int GetTargetRecommendatedPosition()
        {
            List<IReadOnlyUnit> units = GetUnits();

            if (units.Any(unit => GetDistanceToPlayerBase(unit) <= GetDistanceToEnemyBase(unit)))
            {
                Vector2Int dirEnemy = _runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId] - _runtimeModel.RoMap.Bases[RuntimeModel.PlayerId];
                return _runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId] + dirEnemy.SignOrZero();
            }
            else if (units.Any())
            {
                units.Sort(CompareDistanceToPlayerBase);
                return GetPositionOnPathAtRange(units[0], units[0].Config.AttackRange);
            }
            else
            {
                return _runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId];
            }
        }

        private Vector2Int GetPositionOnPathAtRange(IReadOnlyUnit unit, float range)
        {
            if (unit.ActivePath != null)
            {
                foreach (var pos in unit.ActivePath.GetPath())
                {
                    if (Vector2Int.Distance(pos, unit.Pos) <= range) 
                        return pos;
                }
            }
            return unit.Pos;
        }

        private List<IReadOnlyUnit> GetUnits()
        {
            return _runtimeModel.RoBotUnits.ToList();
        }
        private float GetDistanceToPlayerBase(IReadOnlyUnit Unit)
        {
            return Vector2Int.Distance(Unit.Pos, _runtimeModel.RoMap.Bases[RuntimeModel.PlayerId]);
        }
        private float GetDistanceToEnemyBase(IReadOnlyUnit Unit)
        {
            return Vector2Int.Distance(Unit.Pos, _runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId]);
        }
        private int CompareDistanceToPlayerBase(IReadOnlyUnit a, IReadOnlyUnit b)
        {
            float distanceA = GetDistanceToPlayerBase(a);
            float distanceB = GetDistanceToPlayerBase(b);

            return distanceA.CompareTo(distanceB);
        }
        private int CompareHealthEnemy(IReadOnlyUnit a, IReadOnlyUnit b)
        {
            return a.Health.CompareTo(b.Health);
        }
    }


}
