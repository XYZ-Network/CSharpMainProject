using System.Collections.Generic;
using UnityEngine;

namespace UnitBrains.Player
{
    public class ThirdUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Ironclad Behemoth";

        private enum State
        {
            Moving,
            Attacking,
            Switching
        }

        private State _currentState = State.Moving;
        private float _switchTimer = 0f;

        public override void Update(float deltaTime, float time)
        {
            if (_switchTimer > 0)
            {
                _switchTimer -= deltaTime;
                if (_switchTimer <= 0)
                {
                    // «авершили переход Ч переключаемс€ на нужное состо€ние
                    _currentState = HasTargetsInRange() ? State.Attacking : State.Moving;
                }

                return; // пока в переходе Ч ничего не делаем
            }

            if (HasTargetsInRange())
            {
                if (_currentState != State.Attacking)
                {
                    // ѕереход из движени€ в атаку
                    _currentState = State.Switching;
                    _switchTimer = 0.08f;
                    return;
                }

                // ¬ режиме атаки Ч стрел€ем (будет вызван GetProjectiles)
            }
            else
            {
                if (_currentState != State.Moving)
                {
                    // ѕереход из атаки в движение
                    _currentState = State.Switching;
                    _switchTimer = 1f;
                    return;
                }

                // ¬ режиме движени€ Ч система вызовет GetNextStep
            }
        }

        public override Vector2Int GetNextStep()
        {
            // ≈сли в переходе или в атаке Ч стоим
            if (_switchTimer > 0 || _currentState != State.Moving)
                return unit.Pos;

            return base.GetNextStep();
        }

        public override List<Model.Runtime.Projectiles.BaseProjectile> GetProjectiles()
        {
            // ≈сли в переходе или не в атаке Ч не стрел€ем
            if (_switchTimer > 0 || _currentState != State.Attacking)
                return new List<Model.Runtime.Projectiles.BaseProjectile>();

            return base.GetProjectiles();
        }
    }
}






