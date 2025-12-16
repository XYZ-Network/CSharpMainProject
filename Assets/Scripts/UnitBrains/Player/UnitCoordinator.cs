using System.Linq;
using Model;
using Model.Runtime.ReadOnly;
using UnityEngine;
using Utilities;

namespace UnitBrains.Player
{
    public class UnitCoordinator
    {
        private readonly RuntimeModel _runtimeModel;
        private readonly TimeUtil _timeUtil;

        private readonly int _halfMapWidth;
        private readonly bool _isPlayerOnLeftSide;

        public Vector2Int Target { get; private set; }
        public Vector2Int Destination { get; private set; }

        public UnitCoordinator(RuntimeModel runtimeModel, TimeUtil timeUtil)
        {
            _runtimeModel = runtimeModel;
            _halfMapWidth = _runtimeModel.RoMap.Width / 2;
            _isPlayerOnLeftSide = _runtimeModel.RoMap.Bases[RuntimeModel.PlayerId].y < _halfMapWidth;
            
            _timeUtil = timeUtil;
            _timeUtil.AddFixedUpdateAction(Update);
        }

        ~UnitCoordinator()
        {
            _timeUtil.RemoveFixedUpdateAction(Update);
        }

        private void Update(float deltaTime)
        {
            UpdateTarget();
            UpdateDestination();
        }

        private void UpdateTarget()
        {
            // Default target is bot base
            Target = _runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId];
            
            var enemies = _runtimeModel.RoBotUnits.ToList();
            if (!enemies.Any())
                return;
            enemies.Sort(CompareByHealth);
            Target = enemies.First().Pos;
            
            var enemiesOnPlayerSide = enemies.Where(IsTargetOnPlayerSide).ToList();
            if (!enemiesOnPlayerSide.Any())
                return;
            enemiesOnPlayerSide.Sort(CompareByDistanceToPlayerBase);
            Target = enemiesOnPlayerSide.First().Pos;
        }

        private void UpdateDestination()
        {
            // Default destination is bot base
            Destination = _runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId];
            
            var enemies = _runtimeModel.RoBotUnits.ToList();
            if (!enemies.Any())
                return;
            enemies.Sort(CompareByDistanceToPlayerBase);
            Destination = enemies.First().Pos;
            
            var enemiesOnPlayerSide = enemies.Where(IsTargetOnPlayerSide).ToList();
            if (!enemiesOnPlayerSide.Any())
                return;
            Destination = _runtimeModel.RoMap.Bases[RuntimeModel.PlayerId];
        }

        private int CompareByHealth(IReadOnlyUnit a, IReadOnlyUnit b) =>
            a.Health.CompareTo(b.Health);

        private bool IsTargetOnPlayerSide(IReadOnlyUnit target) =>
            (target.Pos.y < _halfMapWidth && _isPlayerOnLeftSide)
            || (target.Pos.y >= _halfMapWidth && !_isPlayerOnLeftSide);

        private int CompareByDistanceToPlayerBase(IReadOnlyUnit a, IReadOnlyUnit b)
        {
            var distanceA = DistanceToPlayerBase(a);
            var distanceB = DistanceToPlayerBase(b);
            return distanceA.CompareTo(distanceB);

            float DistanceToPlayerBase(IReadOnlyUnit target) =>
                Vector2Int.Distance(target.Pos, _runtimeModel.RoMap.Bases[RuntimeModel.PlayerId]);
        }
    }
}