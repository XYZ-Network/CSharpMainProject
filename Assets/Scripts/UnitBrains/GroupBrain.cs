
using System.Linq;
using Model;
using UnityEngine;
using Utilities;

namespace UnitBrains
{
    public class GroupBrain
    {
        private readonly bool _isPlayerGroupBrain;

        private readonly IReadOnlyRuntimeModel _runtimeModel;
        private readonly TimeUtil _timeUtil;

        private Vector2Int _opponentBasePos;
        private Vector2Int _ownBasePos;

        public Vector2Int RecommendTarget { get; private set; }
        public Vector2Int RecommendPoint { get; private set; }

        public GroupBrain(bool isPlayerGroupBrain)
        {
            _isPlayerGroupBrain = isPlayerGroupBrain;

            _runtimeModel = ServiceLocator.Get<IReadOnlyRuntimeModel>();

            _timeUtil = ServiceLocator.Get<TimeUtil>();
            _timeUtil.AddFixedUpdateAction(CalcRecommend);

            if (_isPlayerGroupBrain)
            {
                _opponentBasePos = _runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId];
                _ownBasePos = _runtimeModel.RoMap.Bases[RuntimeModel.PlayerId];
            }
            else
            {
                _opponentBasePos = _runtimeModel.RoMap.Bases[RuntimeModel.PlayerId];
                _ownBasePos = _runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId];
            }

            RecommendTarget = _opponentBasePos;
            RecommendPoint = _opponentBasePos;
        }

        ~GroupBrain()
        {
            _timeUtil.RemoveFixedUpdateAction(CalcRecommend);
        }

        public void CalcRecommend(float deltaTime)
        {
            var oppenentUnits =
                _isPlayerGroupBrain ? _runtimeModel.RoBotUnits.ToList() : _runtimeModel.RoPlayerUnits.ToList();

            if (!oppenentUnits.Any())
            {
                RecommendTarget = _opponentBasePos;
                RecommendPoint = _opponentBasePos;
                return;
            }

            Vector2Int lowestHealthTarget = oppenentUnits[0].Pos;
            int lowestHealthValue = oppenentUnits[0].Health;

            Vector2Int nearestToOwmBaseTarget = oppenentUnits[0].Pos;
            float nearestToOwnBaseDist = float.MaxValue;

            bool hasOpponentInOwnZone = false;

            foreach (var oppUnit in oppenentUnits)
            {
                if (oppUnit.Health < lowestHealthValue)
                {
                    lowestHealthValue = oppUnit.Health;
                    lowestHealthTarget = oppUnit.Pos;
                }

                if ((oppUnit.Pos-_ownBasePos).sqrMagnitude < nearestToOwnBaseDist)
                {
                    nearestToOwnBaseDist = (oppUnit.Pos-_ownBasePos).sqrMagnitude;
                    nearestToOwmBaseTarget = oppUnit.Pos;
                }

                if ((oppUnit.Pos-_ownBasePos).sqrMagnitude < (oppUnit.Pos-_opponentBasePos).sqrMagnitude)
                {
                    hasOpponentInOwnZone = true;
                }
            }

            if (hasOpponentInOwnZone)
            {
                RecommendTarget = nearestToOwmBaseTarget;
                RecommendPoint = _ownBasePos;
            }
            else
            {
                RecommendTarget = lowestHealthTarget;
                RecommendPoint = nearestToOwmBaseTarget;
            }
        }
    }
}
