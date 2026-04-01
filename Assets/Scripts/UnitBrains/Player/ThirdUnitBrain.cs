using System.Collections.Generic;
using UnityEngine;
using Model.Runtime;

namespace UnitBrains.Player
{
    public class ThirdUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Ironclad Behemoth";

        private bool _isMoving = true;
        private bool _isSwitching = false;
        private float _switchTimer = 0f;

        private const float SwitchDuration = 1f;

        public override void Update(float deltaTime, float time)
        {
            base.Update(deltaTime, time);

            if (_isSwitching)
            {
                _switchTimer += deltaTime;

                if (_switchTimer >= SwitchDuration)
                {
                    _switchTimer = 0f;
                    _isSwitching = false;
                    
                    _isMoving = !_isMoving;
                }
            }
            else
            {
                _isSwitching = true;
            }
        }

        protected override List<Vector2Int> SelectTargets()
        {
            if (_isMoving || _isSwitching)
                return new List<Vector2Int>();

            return base.SelectTargets();
        }

        public override Vector2Int GetNextStep()
        {
            if (!_isMoving || _isSwitching)
                return unit.Pos;

            return base.GetNextStep();
        }
    }
}