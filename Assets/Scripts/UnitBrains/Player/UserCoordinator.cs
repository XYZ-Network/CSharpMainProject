using Assets.Scripts.UnitBrains.Player;
using Model;
using System.Linq;
using UnityEngine;
using Utilities;

namespace UnitBrains.Player
{
    public class UnitCoordinator : IUnitCoordinator
    {
        private readonly IReadOnlyRuntimeModel _model;
        private readonly TimeUtil _timeUtil;
        private readonly int _ownId, _enemyId;
        private float _standardAttackRange;

        public float StandardAttackRange => _standardAttackRange;
        public Vector2Int RecommendedTarget { get; private set; }
        public Vector2Int RecommendedPoint { get; private set; }

        public UnitCoordinator(IReadOnlyRuntimeModel model, TimeUtil timeUtil,
            int ownPlayerId, int enemyPlayerId)
        {
            _model = model;
            _timeUtil = timeUtil;
            _ownId = ownPlayerId;
            _enemyId = enemyPlayerId;

            InitializeAttackRange();
            _timeUtil.AddFixedUpdateAction(UpdateRecommendations);
        }

        private void InitializeAttackRange()
        {
            var myUnits = _model.RoUnits
                .Where(u => u.Config.IsPlayerUnit == (_ownId == RuntimeModel.PlayerId))
                .ToList();
            _standardAttackRange = myUnits.Count == 0
                ? 1f
                : myUnits.Min(u => u.Config.AttackRange);
        }

        private void UpdateRecommendations(float dt)
        {
            var ownBase = _model.RoMap.Bases[_ownId];
            var enemyBase = _model.RoMap.Bases[_enemyId];
            var enemies = (_ownId == RuntimeModel.PlayerId
                ? _model.RoBotUnits
                : _model.RoUnits.Where(u => !u.Config.IsPlayerUnit))
                .ToList();

            if (!enemies.Any())
            {
                RecommendedTarget = enemyBase;
                RecommendedPoint = enemyBase;
                return;
            }

            bool enemyOnOurHalf = enemies.Any(e
                => Vector2Int.Distance(e.Pos, ownBase)
                 < Vector2Int.Distance(e.Pos, enemyBase));

            if (enemyOnOurHalf)
            {
                var closest = enemies
                    .OrderBy(e => Vector2Int.Distance(e.Pos, ownBase))
                    .First();
                RecommendedTarget = closest.Pos;
                RecommendedPoint = ownBase;
            }
            else
            {
                var ordered = enemies
                    .OrderBy(e => Vector2Int.Distance(e.Pos, ownBase))
                    .ThenBy(e => e.Health)
                    .ToList();
                var weakest = ordered.Aggregate((a, b) => a.Health < b.Health ? a : b);

                RecommendedTarget = weakest.Pos;
                var nearest = ordered.First().Pos;
                var dir = (Vector2)(nearest - ownBase);
                RecommendedPoint = dir == Vector2.zero
                    ? nearest
                    : Vector2Int.RoundToInt((Vector2)nearest - dir.normalized * _standardAttackRange);
            }
        }
    }
}