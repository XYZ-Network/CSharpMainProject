using System.Collections.Generic;
using Model;
using Model.Runtime.Projectiles;
using UnityEngine;
using Utilities;

namespace UnitBrains.Player
{
    enum State
    {
        Move,
        Attack,
    }

    public class ThirdUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Ironclad Behemoth";
        private State _currentState = State.Move;
        private bool _status;
        private float _forUnitTime = 1.0f;
        private float _timer;

        // Дополнительная переменная для времени атаки
        private float _attackDuration = 1.0f; // Время в которое юнит останется в момент состояния атаки
        private float _attackTimer;

        public override Vector2Int GetNextStep()
        {
            Vector2Int targetPosition = base.GetNextStep();

            if (_currentState == State.Attack)
            {
                // Проверка на окончание атаки
                if (_attackTimer >= _attackDuration)
                {
                    _currentState = State.Move;
                    _attackTimer = 0f; // Сброс таймера атаки
                }
                else
                {
                    _attackTimer += Time.deltaTime; // Увеличиваю таймер атаки
                }
            }

            if (targetPosition == unit.Pos && _currentState == State.Move)
            {
                _status = true;
                _currentState = State.Attack;
            }
            else if (_currentState == State.Attack && targetPosition != unit.Pos)
            {
                _status = true;
                _currentState = State.Move;
            }

            return _status ? unit.Pos : targetPosition;
        }

        protected override List<Vector2Int> SelectTargets()
        {
            if (_currentState == State.Attack)
                return base.SelectTargets();

            return new List<Vector2Int>();
        }

        public override void Update(float deltaTime, float time)
        {
            if (_status)
            {
                _timer += Time.deltaTime;

                if (_timer >= _forUnitTime)
                {
                    _timer = 0f;
                    _status = false;
                }
            }

            base.Update(deltaTime, time);
        }
    }
}