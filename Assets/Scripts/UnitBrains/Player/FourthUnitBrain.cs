using System;
using System.Collections.Generic;
using System.Linq;
using Model;
using Model.Runtime.Buffs;
using Model.Runtime.ReadOnly;
using UnityEngine;
using Utilities;
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
        private BuffSystem _buffSystem = ServiceLocator.Get<BuffSystem>();
        
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

            if (_moveDelayPassed && _unitToBuff != null && IsTargetInRange(_unitToBuff.Pos))
            {
                switch (_unitToBuff.Config.Name)
                {
                    case "Ironclad Behemoth":
                        _buffSystem.AddBuffToUnit(new AttackRangeBuff(_unitToBuff, 10, 3));
                        break;
                    case "Cobra Commando":
                        _buffSystem.AddBuffToUnit(new MultiShotBuff(_unitToBuff, 10, 1));
                        break;
                    default:
                        _buffSystem.AddBuffToUnit(new InvulnerabilityBuff(_unitToBuff, 10, 1));
                        break;
                }
                
                _lastBuffDateTime = DateTime.Now;
                _moveDelayPassed = false;
                _unitToBuff = null;     
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