using Model;
using Model.Runtime.Projectiles;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnitBrains.Player;
using UnityEngine;
using Utilities;
using static UnityEngine.GraphicsBuffer;

namespace UnitBrains.Player
{
    public class ThirdUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Ironclad Behemoth";
        
        private enum UnitState
        {
            Moving,
            SwitchingAfterAttacking,
            Attacking,
            SwitchingAfterMoving
        }

        private UnitState _currentState = UnitState.Moving;
        private float _stateTimer;
        private static readonly float SwitchDuration = 0.1f;
        public override void Update(float deltaTime, float time)
        {
            base.Update(deltaTime, time);

            bool hasTargetsNow = HasTargetsInRange();
            _stateTimer -= deltaTime;
            
            switch (_currentState)
            {
                case UnitState.Moving:
                    if (hasTargetsNow)
                    {
                        EnterState(UnitState.SwitchingAfterMoving, SwitchDuration);
                    }
                    break;

                case UnitState.SwitchingAfterMoving:
                    if (_stateTimer <= 0f)
                    {
                        EnterState(UnitState.Attacking, 0f);
                    }
                    break;

                case UnitState.Attacking:
                    if (!hasTargetsNow)
                    {
                        EnterState(UnitState.SwitchingAfterAttacking, SwitchDuration);
                    }
                    break;

                case UnitState.SwitchingAfterAttacking:
                    if (_stateTimer <= 0f)
                    {
                        EnterState(UnitState.Moving, 0f);
                    }
                    break;
            }
        }

        private void EnterState(UnitState newState, float duration)
        {
            _currentState = newState;
            _stateTimer = duration;
        }

        public override Vector2Int GetNextStep()
        {
            if (_currentState != UnitState.Moving)
            {
                return unit.Pos;
            }
            var nextStep = base.GetNextStep();
            return nextStep;
        }

        protected override List<Vector2Int> SelectTargets()
        {
            if (_currentState != UnitState.Attacking)
            {
                return new List<Vector2Int>();
            }
            var targets = base.SelectTargets();
            return targets;
        }
        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            base.GenerateProjectiles(forTarget, intoList);
        }
    }
}