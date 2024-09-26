using Model;
using Model.Runtime;
using Model.Runtime.ReadOnly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Utilities;
using static UnityEditor.PlayerSettings;
using static UnityEngine.UI.CanvasScaler;
using Unit = Model.Runtime.Unit;

namespace Assets.Scripts.Utilities
{
    public class UnitCoordinator
    {
        private readonly IReadOnlyRuntimeModel _runtimeModel;
        private readonly TimeUtil _timeUtil;
        private int _playerId;
        public IReadOnlyUnit recommendTarget = null;


        public UnitCoordinator(IReadOnlyRuntimeModel runtimeModel, TimeUtil timeUtil, int playerId)
        {
            _runtimeModel = runtimeModel;
            _timeUtil = timeUtil;
            _playerId = playerId;

            _timeUtil.AddUpdateAction(GetRecommendedTarget);
        }

        private void GetRecommendedTarget(float deltaTime)
        {
            recommendTarget = null;

            if (_runtimeModel.RoMap == null)
            {
                return;
            }

            int firstHalfMapX = _runtimeModel.RoMap.Width / 2;
            float closedDist = float.MaxValue;
            IEnumerable<IReadOnlyUnit> units = _playerId == RuntimeModel.PlayerId ? _runtimeModel.RoBotUnits : _runtimeModel.RoPlayerUnits;
            List<IReadOnlyUnit> Targets = units.ToList();

            foreach (IReadOnlyUnit target in Targets)
            {
                if (target.Pos.x < firstHalfMapX && closedDist > (target.Pos - _runtimeModel.RoMap.Bases[RuntimeModel.PlayerId]).magnitude)
                {
                    closedDist = (target.Pos - _runtimeModel.RoMap.Bases[RuntimeModel.PlayerId]).magnitude;
                    recommendTarget = target;
                }
            }

            if (recommendTarget != null)
            {
                return;
            }

            int minHP = int.MaxValue;
            foreach (IReadOnlyUnit target in Targets)
            {
                if (minHP > target.Health)
                {
                    minHP = target.Health;
                    recommendTarget = target;
                }
            }
        }
    }
}