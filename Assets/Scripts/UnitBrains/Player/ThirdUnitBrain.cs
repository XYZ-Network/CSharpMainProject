using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnitBrains.Player;
using UnityEngine;

namespace UnitBrains.Player
{
    public class ThirdUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Ironclad Behemoth";

        private enum MovementState
        {
            Moving,
            Standing,
            TransitioningToMove,
            TransitioningToStand
        }

        private MovementState _currentState = MovementState.Moving;
        private float _stateChangeTime = 0f;
        private const float TransitionDuration = 1f;

        public override Vector2Int GetNextStep() { 

            if (_currentState != MovementState.Moving)
            {
                return unit.Pos;
            }
            return base.GetNextStep(); 
        }
        
        protected override List<Vector2Int> SelectTargets() { 

            if (_currentState != MovementState.Standing)
            {
                return new List<Vector2Int>();
            }
            return base.SelectTargets(); 
        }

        public override void Update(float deltaTime, float time) 
        {
            if (_currentState == MovementState.TransitioningToMove || _currentState == MovementState.TransitioningToStand)
            {
                if (time - _stateChangeTime >= TransitionDuration)
                {
                    if (_currentState == MovementState.TransitioningToMove) _currentState = MovementState.Moving;
                    else _currentState = MovementState.Standing;
                }

                return;
            }

            if (_currentState == MovementState.Moving && HasTargetsInRange())
            {
                _currentState = MovementState.TransitioningToStand;
                _stateChangeTime = time;
            }
            else if (_currentState == MovementState.Standing && !HasTargetsInRange())
            {
                _currentState = MovementState.TransitioningToMove;
                _stateChangeTime = time;
            }
        }
    }
}
