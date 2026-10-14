using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitBrains.Player
{
    public class ThirdUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Ironclad Behemoth";

        public enum PlayerCondition
        {
            Moving,
            Attacking,
            Transitioning
        }

        PlayerCondition _condition = PlayerCondition.Moving;
        const float _transitionDuration = 1f;
        float _transitionTimer;

        public override void Update(float deltaTime, float time)
        {
            bool inRange = HasTargetsInRange();
            if (_condition == PlayerCondition.Transitioning)
            {
                _transitionTimer += time;
                if (_transitionTimer >= _transitionDuration)
                {
                    _transitionTimer = 0f;
                    _condition = inRange ? PlayerCondition.Attacking : PlayerCondition.Moving;
                }
                return;
            }
            if (inRange && _condition != PlayerCondition.Attacking)
            {
                _condition = PlayerCondition.Transitioning;
                _transitionTimer = 0f;
            } else if (!inRange && _condition != PlayerCondition.Moving)
            {
                _condition = PlayerCondition.Transitioning;
                _transitionTimer = 0f;
            }
        }

        public override Vector2Int GetNextStep()
        {
            if (_condition != PlayerCondition.Moving)
                return unit.Pos;
            return base.GetNextStep();
        }

        protected override List<Vector2Int> SelectTargets()
        {
            if (_condition != PlayerCondition.Attacking)
                return new List<Vector2Int>(); 
            return base.SelectTargets();
        }
    }
}