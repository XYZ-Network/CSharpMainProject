using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Playables;
using Utilities;
using Model.Runtime;
using Model.Runtime.ReadOnly;
using UnityEngine;
using static UnityEngine.UI.CanvasScaler;
using UnitBrains.Pathfinding;

namespace Assets.Scripts.UnitBrains.Player
{
    public class TargetAdviser
    {

        private static TargetAdviser _instance;
        private IReadOnlyRuntimeModel _runtimeModel;

        public Vector2Int PlayerBase => _runtimeModel.RoMap.Bases[RuntimeModel.PlayerId];
        public Vector2Int EnemyBase => _runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId];

        public IReadOnlyUnit RecomendedTarget { get; private set; }
        public Vector2Int RecomendedPosition { get; private set; }
        public static TargetAdviser Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TargetAdviser();
                }
                return _instance;
            }
        }

        private TargetAdviser()
        {
            _runtimeModel = ServiceLocator.Get<IReadOnlyRuntimeModel>();

            var timeUtil = ServiceLocator.Get<TimeUtil>();

            timeUtil.AddFixedUpdateAction(Update);
            RecomendedPosition = EnemyBase;

        }
        private void Update(float deltaTime)
        {
            RecomendedTarget = CalculateTargetUnit();
            RecomendedPosition = CalculateTargetPosition();
        }

        private IReadOnlyUnit CalculateTargetUnit()
        {
            var botUnits = _runtimeModel.RoBotUnits.ToList();
            if (botUnits.Count == 0)
            {
                return null;
            }
            else if (botUnits.Any(IsCloserToPlayerBase))
            {
                botUnits.Sort(CompareByDistanceToPlayerBase);
            }
            else
            {
                botUnits.Sort(CompareByHealth);
            }
            return botUnits.Any() ? botUnits[0] : null;
        }
        private Vector2Int CalculateTargetPosition()
        {
            var botUnits = _runtimeModel.RoBotUnits.ToList();
            if (botUnits.Any(IsCloserToPlayerBase))
            {
                var toEnemy = EnemyBase - PlayerBase;
                return PlayerBase + toEnemy.SignOrZero();
            }
            else if (botUnits.Any())
            {
                botUnits.Sort(CompareByDistanceToPlayerBase);
                var selectedUnit = botUnits[0];
                var path = new AStarUnitPath(_runtimeModel, PlayerBase, selectedUnit.Pos);
                return GetPositionOnPathAtRange(path, selectedUnit.Config.AttackRange);
            }
            else
            {
                return EnemyBase;
            }
        }

        private Vector2Int GetPositionOnPathAtRange(AStarUnitPath path, float range)
        {
            foreach (var pos in path.GetPath())
            {
                if (Vector2Int.Distance(pos, path.EndPoint) <= range)
                {
                    return pos;
                }
            }
            return path.EndPoint;
        }

        protected bool IsCloserToPlayerBase(IReadOnlyUnit unit)
        {
            return DistanceToOwnBase(unit) <= DistanceToBotBase(unit);
        }

        protected float DistanceToOwnBase(IReadOnlyUnit fromPos) =>
            Vector2Int.Distance(fromPos.Pos, _runtimeModel.RoMap.Bases[RuntimeModel.PlayerId]);
        protected float DistanceToBotBase(IReadOnlyUnit fromPos) =>
            Vector2Int.Distance(fromPos.Pos, _runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId]);

        private int CompareByDistanceToPlayerBase(IReadOnlyUnit a, IReadOnlyUnit b)
        {
            var distanceA = DistanceToOwnBase(a);
            var distanceB = DistanceToOwnBase(b);
            return distanceA.CompareTo(distanceB);
        }

        private int CompareByHealth(IReadOnlyUnit a, IReadOnlyUnit b)
        {
            return a.Health - b.Health;
        }
    }
}
