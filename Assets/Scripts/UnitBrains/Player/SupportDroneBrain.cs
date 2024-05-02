using System.Collections.Generic;
using Controller;
using UnityEngine;
using Utilities;

namespace UnitBrains.Player
{
    public class SupportDroneBrain : BaseUnitBrain
    {
        public override string TargetUnitName => "Support Drone";
        
        private StatusEffectsController _statusEffectsController;
        private float _buffCooldown = 3f;
        private float _stopDuration = 0.5f;
        private float _lastBuffTime = 0f;
        private float _unitStopTime = 0f;
        private bool _buffing = false;
        private bool _moving = true;

        public SupportDroneBrain()
        {
            _statusEffectsController = ServiceLocator.Get<StatusEffectsController>();
        }
        
        
        protected override List<Vector2Int> SelectTargets() => new ();
        
        
        public override Vector2Int GetNextStep()
        {
            return _moving ? base.GetNextStep() : unit.Pos;
        }
        
        
        public override void Update(float deltaTime, float time)
        {
            if (CooldownEnded())
            {
                if (_buffing) 
                    BuffAlliedUnits();
                
                StopMoving();
                _buffing = ReadyToBuff();
            }
            else
            {
                _buffing = false;
                _moving = true;
            }
        }
        
        
        private void BuffAlliedUnits()
        {
            var alliesInRadius = GetUnitsInRadius(unit.Config.AttackRange, false);
            
            foreach (var ally in alliesInRadius) 
            { 
                _statusEffectsController.ApplyEffectOnUnit(ally, EffectType.Buff);
            }
            
            _lastBuffTime = Time.time;
        }
        
        
        public void StopMoving()
        {
            if (_moving)
            {
                _moving = false;
                _unitStopTime = Time.time;
            }
        }
        
        
        private bool ReadyToBuff()
        {
            return (Time.time - _unitStopTime >= _stopDuration) && _unitStopTime != 0;
        }
        
        
        private bool CooldownEnded()
        {
            return (Time.time - _lastBuffTime >= _buffCooldown) || _lastBuffTime == 0f;
        }
    }
}