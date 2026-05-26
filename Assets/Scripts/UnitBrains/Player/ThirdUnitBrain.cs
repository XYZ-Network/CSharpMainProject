using System.Collections.Generic;
using UnityEngine;


namespace UnitBrains.Player
{
    public class ThirdUnitBrain : DefaultPlayerUnitBrain
    {
        private enum BehemothState
        {
            Moving,
            SwitchingToAttack,
            SwitchingToMove,
            Attacking
        }

        public override string TargetUnitName => "Ironclad Behemoth";
        private BehemothState _state = BehemothState.Moving;
        private float _switchStartTime = 0f;
        private const float SwitchDuration = 1f;

        public override void Update(float deltaTime, float time)
        {
            base.Update(deltaTime, time);
            switch (_state)
            {
                case BehemothState.Moving:
                    if (HasTargetsInRange())
                    {
                        _state = BehemothState.SwitchingToAttack;
                        _switchStartTime = time;
                    }
                    break;

                case BehemothState.SwitchingToAttack:
                    if (time - _switchStartTime >= SwitchDuration)
                    {
                        _state = BehemothState.Attacking;
                    }
                    break;

                case BehemothState.SwitchingToMove:
                    if (time - _switchStartTime >= SwitchDuration)
                    { 
                        _state = BehemothState.Moving;
                    }
                    break;

                case BehemothState.Attacking:
                    if (!HasTargetsInRange())
                    {
                        _state = BehemothState.SwitchingToMove;
                        _switchStartTime = time;
                    }
                    break;
            }
        }

        public override Vector2Int GetNextStep()
        {
            if (_state != BehemothState.Moving)
                return unit.Pos;

            return base.GetNextStep();
        }

        protected override List<Vector2Int> SelectTargets()
        {
            if (_state != BehemothState.Attacking)
                return new List<Vector2Int>();

            return base.SelectTargets();
        }
    }
}