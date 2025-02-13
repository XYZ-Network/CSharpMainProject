
using System.Linq;
using Model;
using UnityEngine;
using Utilities;
using static UnityEditor.PlayerSettings;

namespace UnitBrains.Player
{
    public class GroupBrain
    {
        private static GroupBrain _instance;

        private IReadOnlyRuntimeModel _runtimeModel;
        private TimeUtil _timeUtil;

        private Vector2Int _enemyBasePos;
        private Vector2Int _playerBasePos;

        public Vector2Int RecommendTarget { get; private set; }
        public Vector2Int RecommendPoint { get; private set; }

        private GroupBrain()
        {
            _runtimeModel = ServiceLocator.Get<IReadOnlyRuntimeModel>();

            RecommendTarget = _runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId];
            RecommendPoint = _runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId];

            _timeUtil = ServiceLocator.Get<TimeUtil>();
            _timeUtil.AddFixedUpdateAction(CalcRecommend);
        }

        public static GroupBrain GetInstance()
        {
            if (_instance == null)
                _instance = new GroupBrain();
            return _instance;
        }

        ~GroupBrain()
        {
            _timeUtil.RemoveFixedUpdateAction(CalcRecommend);
        }

        public void CalcRecommend(float deltaTime)
        {
            _enemyBasePos = _runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId];
            _playerBasePos = _runtimeModel.RoMap.Bases[RuntimeModel.PlayerId];

            var botUnits = _runtimeModel.RoBotUnits.ToList();

            if (!botUnits.Any())
            {
                RecommendTarget = _enemyBasePos;
                RecommendPoint = _enemyBasePos;
                return;
            }

            Vector2Int lowestHealthTarget = botUnits[0].Pos;
            int lowestHealthValue = botUnits[0].Health;

            Vector2Int nearestToPlayerBaseTarget = botUnits[0].Pos;
            float nearestToPlayerBaseDist = float.MaxValue;

            bool hasEnemyInPlayerZone = false;

            foreach (var botUnit in _runtimeModel.RoBotUnits)
            {
                if (botUnit.Health < lowestHealthValue)
                {
                    lowestHealthValue = botUnit.Health;
                    lowestHealthTarget = botUnit.Pos;
                }

                if (Vector2Int.Distance(botUnit.Pos, _playerBasePos) < nearestToPlayerBaseDist)
                {
                    nearestToPlayerBaseDist = Vector2Int.Distance(botUnit.Pos, _playerBasePos);
                    nearestToPlayerBaseTarget = botUnit.Pos;
                }

                if (Vector2Int.Distance(botUnit.Pos, _playerBasePos)
                    < Vector2Int.Distance(botUnit.Pos, _enemyBasePos))
                {
                    hasEnemyInPlayerZone = true;
                }
            }

            if (hasEnemyInPlayerZone)
            {
                RecommendTarget = nearestToPlayerBaseTarget;
                RecommendPoint = _playerBasePos;
            }
            else
            {
                RecommendTarget = lowestHealthTarget;
                RecommendPoint = nearestToPlayerBaseTarget;
            }
        }
    }
}
