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
        private float _forUnitTime = 0.1f;
        private float _timer;

        public override Vector2Int GetNextStep()
        {
            Vector2Int targetPosition = base.GetNextStep();

            if (targetPosition == unit.Pos)
            {
                if (_currentState == State.Move)
                    _status = true;

                _currentState = State.Attack;
            }
            else
            {
                if (_currentState == State.Attack)
                    _status = true;

                _currentState = State.Move;
            }

            return _status ? unit.Pos : targetPosition;
        }

        protected override List<Vector2Int> SelectTargets()
        {
            if (_status)
                return new List<Vector2Int>();

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
