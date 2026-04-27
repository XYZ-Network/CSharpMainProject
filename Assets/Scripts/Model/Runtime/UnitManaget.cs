using System.Linq;
using Model;
using Model.Runtime.ReadOnly;
using UnityEngine;
using Utilities;

namespace Assets.Scripts.Model.Runtime
{
    public class UnitManager
    {
        private static UnitManager _instance;
        public static UnitManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new UnitManager();
                return _instance;
            }
        }

        private readonly IReadOnlyRuntimeModel _runtimeModel;
        private readonly TimeUtil _timeUtil;

        public IReadOnlyUnit RecommendedTarget { get; private set; }
        public Vector2Int RecommendedPosition { get; private set; }

        private UnitManager()
        {
            _runtimeModel = ServiceLocator.Get<IReadOnlyRuntimeModel>();
            _timeUtil = ServiceLocator.Get<TimeUtil>();

            _timeUtil.AddUpdateAction(OnUpdate);
        }

        private void OnUpdate(float deltaTime)
        {
            UpdateLogic();
        }

        private void UpdateLogic()
        {
            var enemies = _runtimeModel.RoBotUnits.ToList();

            if (enemies.Count == 0)
            {
                RecommendedTarget = null;
                return;
            }

            var basePos = _runtimeModel.RoMap.Bases[RuntimeModel.PlayerId];

            var enemiesNearBase = enemies
                .Where(e => Vector2Int.Distance(e.Pos, basePos) < 5)
                .ToList();

            if (enemiesNearBase.Count > 0)
            {
                RecommendedTarget = enemiesNearBase
                    .OrderBy(e => Vector2Int.Distance(e.Pos, basePos))
                    .First();

                var dir = RecommendedTarget.Pos - basePos;
                dir = new Vector2Int(
                    Mathf.Clamp(dir.x, -1, 1),
                    Mathf.Clamp(dir.y, -1, 1)
                );

                RecommendedPosition = basePos + dir;
            }
            else
            {
                RecommendedTarget = enemies
                    .OrderBy(e => e.Health)
                    .First();

                var dir = RecommendedTarget.Pos - basePos;
                dir = new Vector2Int(
                    Mathf.Clamp(dir.x, -1, 1),
                    Mathf.Clamp(dir.y, -1, 1)
                );

                int attackRange = 2; 
                RecommendedPosition = RecommendedTarget.Pos - dir * attackRange;
            }
        }
    }
}