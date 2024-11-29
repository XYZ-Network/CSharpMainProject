using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitBrains.Player;
using UnityEngine;
using Utilities;

namespace UnitBrains.Player
{
    internal class ThirdUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Ironclad Behemoth";
        UnitAction NowUnitAction = UnitAction.Move;
        UnitAction LastUnitAction = UnitAction.Move;
        bool _freezing = false;
        float _freezingTime = 1f;
        float _freezingTimer;

        List<Vector2Int> SelectTargetsResult;
        enum UnitAction
        {
            Move,
            Attack,
            None
        }
        public override Vector2Int GetNextStep()
        {
            if (!_freezing)
            {
                if (NowUnitAction != UnitAction.Attack)
                {
                    return base.GetNextStep();
                }
            }
            return unit.Pos;
        }

        protected override List<Vector2Int> SelectTargets()
        {
            if (!_freezing)
            {
                if (NowUnitAction != UnitAction.Move)
                    return base.SelectTargets();
            }
            return new List<Vector2Int>();
        }
        public override void Update(float deltaTime, float time)
        {

            if (HasTargetsInRange() )
            {
                NowUnitAction = UnitAction.Attack;
            }
            else
            {
                NowUnitAction = UnitAction.Move;
            }
            if ((NowUnitAction == LastUnitAction) && _freezing)
            {
                _freezing = false;
                LastUnitAction = NowUnitAction;
                _freezingTimer = _freezingTime;
                LastUnitAction = UnitAction.None;
            }
            if ((LastUnitAction != NowUnitAction))
            { 
                _freezing = true;
                _freezingTimer -= 0.25f;
                if (_freezingTimer <= 0)
                {
                    _freezing = false;
                    LastUnitAction = NowUnitAction;
                }
            }
            else{
                _freezing = false; 
                LastUnitAction = NowUnitAction;
                _freezingTimer = _freezingTime;
            }
            base.Update(deltaTime, time);
        }
    }
}
