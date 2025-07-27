using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Model;
using Model.Runtime.ReadOnly;
using UnitBrains.Pathfinding;
using UnityEngine;
using Utilities;

namespace UnitBrains.Player
{
    /// <summary>
    /// Struct represents recommendation for specific unit.
    /// </summary>
    public readonly struct UnitRecommendation
    {
        [CanBeNull] public IReadOnlyUnit Target { get; }
        public Vector2Int Zone { get; }

        public UnitRecommendation([CanBeNull] IReadOnlyUnit target, Vector2Int zone)
        {
            Target = target;
            Zone = zone;
        }
    }

    public class UnitCoordinator
    {
        private static UnitCoordinator _instance;

        public static UnitCoordinator GetInstance()
        {
            return _instance ??= new UnitCoordinator();
        }

        private readonly IReadOnlyRuntimeModel _runtimeModel;
        private readonly Dictionary<IReadOnlyUnit, UnitRecommendation> _recommendations = new();
        private readonly Vector2Int _mapCenter;
        private readonly TimeUtil _timeUtil;

        private UnitCoordinator()
        {
            _runtimeModel = ServiceLocator.Get<IReadOnlyRuntimeModel>();
            _mapCenter = new Vector2Int(_runtimeModel.RoMap.Width / 2, _runtimeModel.RoMap.Height / 2);
            _timeUtil = ServiceLocator.Get<TimeUtil>();
            
            // Not sure if it's a good idea to subscribe here in controller, i'd subscribe in some
            // central game controller, but anyway we have to start from something
            _timeUtil.AddFixedUpdateAction(Update);

            // Update, so we'll have recommendation on the first request.
            Update(0);
        }

        ~UnitCoordinator()
        {
            _timeUtil.RemoveFixedUpdateAction(Update);
        }

        private void Update(float deltaTime)
        {
            if (_runtimeModel.Stage != RuntimeModel.GameStage.Simulation)
            {
                return;
            }

            _recommendations.Clear();

            foreach (var unit in _runtimeModel.RoPlayerUnits)
            {
                _recommendations[unit] = ComputeRecommendationFor(unit);
            }
        }

        public UnitRecommendation GetRecommendation(IReadOnlyUnit unit)
        {
            return _recommendations.TryGetValue(unit, out var rec)
                ? rec
                : new UnitRecommendation(null, GetPlayerBase());
        }

        private UnitRecommendation ComputeRecommendationFor(IReadOnlyUnit unit)
        {
            var target = SelectRecommendedTarget();
            var zone = SelectRecommendedZone();
            return new UnitRecommendation(target, zone);
        }

        private IReadOnlyUnit SelectRecommendedTarget()
        {
            var enemies = _runtimeModel.RoBotUnits.ToList();
            if (enemies.Count == 0)
            {
                return null;
            }

            var enemiesOnOurSide = enemies.Where(IsWithinRadius).ToList();

            if (enemiesOnOurSide.Count > 0)
            {
                // TODO: use MinBy in C# 10+
                return enemiesOnOurSide
                    .OrderBy(e => ManhDistance(e.Pos, GetPlayerBase()))
                    .First();
            }

            // TODO: use MinBy in C# 10+
            return enemies
                .OrderBy(e => e.Health)
                .First();
        }

        private Vector2Int SelectRecommendedZone()
        {
            var enemies = _runtimeModel.RoBotUnits.ToList();

            if (enemies.Count == 0)
            {
                return GetBotBase();
            }

            var enemiesCloseToBase = enemies.Where(IsWithinRadius).ToList();

            if (enemiesCloseToBase.Count > 0)
            {
                // Cell in front of our base. Not sure if we should go to free cell, we can target just our base for now
                return MapUtils.FindBestAdjacentTileTowards(_runtimeModel, GetPlayerBase(), GetBotBase());
            }

            var closestEnemy = enemies
                .OrderBy(e => ManhDistance(e.Pos, GetPlayerBase()))
                .FirstOrDefault();

            // Just select unit, as in any case we'll have to find path to it, as
            // this can be behind some obstacle, which we don't want to calculate right now,
            // and also when our unit gets within attack range, it will stop and shoot. 
            return closestEnemy?.Pos ?? GetBotBase();
        }

        private Vector2Int GetPlayerBase()
        {
            return _runtimeModel.RoMap.Bases[RuntimeModel.PlayerId];
        }

        private Vector2Int GetBotBase()
        {
            return _runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId];
        }

        private bool IsWithinRadius(IReadOnlyUnit enemy)
        {
            float radius = Vector2Int.Distance(_mapCenter, GetPlayerBase());

            return Vector2Int.Distance(enemy.Pos, GetPlayerBase()) < radius;
        }

        private int ManhDistance(Vector2Int a, Vector2Int b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }
    }
}