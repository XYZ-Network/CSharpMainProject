using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnitBrains.Player
{
    public class ThirdUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Ironclad Behemoth";

        private static int _switchingTimeSeconds = 1;
        private static DateTime _switchingStartDateTime;
        
        private bool _isSwitching;
        private bool _isSiegeMode;
        
        public override Vector2Int GetNextStep()
        {
            if (NeedToSwitchMode()) StartModeSwitching();
            return _isSwitching || _isSiegeMode ? unit.Pos : base.GetNextStep();
        }
        
        protected override List<Vector2Int> SelectTargets()
        {
            if (NeedToSwitchMode()) StartModeSwitching();
            return _isSwitching || !_isSiegeMode ? new List<Vector2Int>() : base.SelectTargets();
        }

        public override void Update(float deltaTime, float time)
        {
            if (_isSwitching)
            {
                int totalSwitchingTime = (DateTime.Now - _switchingStartDateTime).Seconds;
                if (totalSwitchingTime >= _switchingTimeSeconds) CompleteModeSwitching();
            }
        }

        private bool NeedToSwitchMode() => !_isSwitching && (HasTargetsInRange() != _isSiegeMode);
        
        private void StartModeSwitching()
        {
            _switchingStartDateTime = DateTime.Now;
            _isSwitching = true;
        }

        private void CompleteModeSwitching()
        {
            _isSiegeMode = !_isSiegeMode;
            _isSwitching = false;
        }
        
    }
}