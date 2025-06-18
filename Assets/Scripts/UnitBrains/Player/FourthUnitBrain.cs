using System;
using System.Collections.Generic;
using System.Linq;
using Model;
using Model.Runtime.Buffs;
using Model.Runtime.ReadOnly;
using UnityEngine;
using Random = System.Random;

namespace UnitBrains.Player
{
    public class FourthUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Heaven Blessing";
        
        private static float _moveDelayAfterBuff = 0.5f;
        private static DateTime _lastBuffDateTime;
        private bool _moveDelayPassed;
        private IReadOnlyUnit _unitToBuff;
        private Random _random = new Random();
        
        public override Vector2Int GetNextStep() =>
            _moveDelayPassed ? base.GetNextStep(): unit.Pos;

        public override Vector2Int GetNextStepTarget()
        {
            return _unitToBuff != null ? _unitToBuff.Pos 
                : runtimeModel.RoMap.Bases[RuntimeModel.PlayerId];
        } 
        
        protected override List<Vector2Int> SelectTargets()
        {
            return new();
        }
        
        public override void Update(float deltaTime, float time)
        {
            var units = GetOtherPlayerUnits();
            if (_unitToBuff == null && units.Any())
                _unitToBuff = units.ElementAt(_random.Next(units.Count()));
            
            if (!_moveDelayPassed)
            {
                int totalLastBuffDelay = (DateTime.Now - _lastBuffDateTime).Seconds;
                if (totalLastBuffDelay >= _moveDelayAfterBuff) 
                    _moveDelayPassed = true;
            }

            if (_moveDelayPassed)
            {
                if (_unitToBuff != null && IsTargetInRange(_unitToBuff.Pos))
                {
                    // buff - Invulnerability but only 0.75 AS and MS
                    unit.BuffSystem.AddBuffToUnit(_unitToBuff, new Buff(Buff.BuffType.Invulnerability, 3, 1));
                    unit.BuffSystem.AddBuffToUnit(_unitToBuff, new Buff(Buff.BuffType.MoveSpeed, 3, 0.75f));
                    unit.BuffSystem.AddBuffToUnit(_unitToBuff, new Buff(Buff.BuffType.AttackSpeed, 3, 0.75f));
                    _lastBuffDateTime = DateTime.Now;
                    _moveDelayPassed = false;
                    _unitToBuff = null;
                }
            }
        }
        
        protected IEnumerable<IReadOnlyUnit> GetOtherPlayerUnits()
        {
            return runtimeModel.RoUnits
                .Where(u => u != unit)
                .Where(u => u.Config.IsPlayerUnit == IsPlayerUnitBrain);
        }
        
    }
}