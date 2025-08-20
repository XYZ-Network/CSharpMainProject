using System.Linq;
using Model;
using Model.Runtime.ReadOnly;
using UnityEngine;
using Utilities;

namespace UnitBrains.Coordinator
{
    public class UnitCoordinator
    {
        //private static UnitCoordinator _instance;
        //public static UnitCoordinator Instance => _instance ??= new UnitCoordinator();

        private IReadOnlyRuntimeModel _runtimeModel;

        public Vector2Int? RecommendedTarget { get; private set; }
        public Vector2Int? RecommendedPoint { get; private set; }

        public UnitCoordinator(IReadOnlyRuntimeModel model) 
        {
            _runtimeModel = model;
        }

        //public void Init(IReadOnlyRuntimeModel model, TimeUtil timeUtil)
        //{
        //    if (_runtimeModel != null) return;

        //    _runtimeModel = model;
        //    timeUtil.AddFixedUpdateAction(OnFixedUpdate);
        //}

        private void OnFixedUpdate(float dt)
        {
            if (_runtimeModel == null) return;
            UpdateRecommendations();
        }

        private void UpdateRecommendations()
        {
            var basePos = _runtimeModel.RoMap.Bases[RuntimeModel.PlayerId];
            var enemies = _runtimeModel.RoBotUnits.ToList();

            if (enemies.Count == 0)
            {
                RecommendedTarget = null;
                RecommendedPoint = null;
                return;
            }

            var ourHalf = _runtimeModel.RoMap.Height / 2;
            bool enemyOnOurSide = enemies.Any(e => e.Pos.y < ourHalf);

            if (enemyOnOurSide)
            {
                var closest = enemies.OrderBy(e => (e.Pos - basePos).sqrMagnitude).First();
                RecommendedTarget = closest.Pos;
                RecommendedPoint = basePos + Vector2Int.up;
            }
            else
            {
                var weakest = enemies.OrderBy(e => e.Health).First();
                RecommendedTarget = weakest.Pos;

                var direction = weakest.Pos - basePos;
                direction.Clamp(Vector2Int.one * -1, Vector2Int.one);
                RecommendedPoint = weakest.Pos - direction;
            }
        }
    }
}
