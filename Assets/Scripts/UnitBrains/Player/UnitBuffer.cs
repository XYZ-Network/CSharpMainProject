using Model;
using Model.Runtime;
using Model.Runtime.Projectiles;
using Model.Runtime.ReadOnly;
using System.Collections.Generic;
using System.Linq;
using UnitBrains.Player;
using UnityEngine;
using Utilities;
using View;

namespace UnitBrains.Player
{
    public class BufferUnitBrain : DefaultPlayerUnitBrain
    {
        private float _nextBuffTime;
        private float _buffCooldown = 3f;
        private float _stopDuration = 0.5f;
        private float _stopTimer;
        private bool _isStopped;
        private bool _isBuffing;
        private EffectSystem _effectSystem;
        private VFXView _vfxView;
        private CoroutineHelper _coroutineHelper;

        public override string TargetUnitName => "BufferUnit";
        public override bool IsPlayerUnitBrain => true;

        public BufferUnitBrain()
        {
            _effectSystem = ServiceLocator.Get<EffectSystem>();
            _vfxView = ServiceLocator.Get<VFXView>();
            _coroutineHelper = CoroutineHelper.Instance;
            _nextBuffTime = 0f;
        }

        public override void Update(float deltaTime, float time)
        {
            if (unit.IsDead)
                return;

            
            if (_isStopped)
            {
                _stopTimer -= deltaTime;
                if (_stopTimer <= 0)
                {
                    _isStopped = false;
                    _isBuffing = false;
                }
                return;
            }

            
            if (time >= _nextBuffTime)
            {
                var targetAlly = FindAllyToBuff();
                if (targetAlly != null)
                {
                    StartBuffing(targetAlly, time);
                }
                else
                {
                    _nextBuffTime = time + 1f;
                }
            }
        }

        public override Vector2Int GetNextStep()
        {
           
            if (_isStopped || _isBuffing)
            {
                return unit.Pos;
            }

            
            return base.GetNextStep();
        }

        protected override List<Vector2Int> SelectTargets()
        {
            return new List<Vector2Int>();
        }

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            
            return;
        }

        private void StartBuffing(IReadOnlyUnit target, float time)
        {
            
            _isStopped = true;
            _stopTimer = _stopDuration;
            _isBuffing = true;

            
            _coroutineHelper.StartDelayedAction(_stopDuration, () =>
            {
                if (!unit.IsDead && target != null)
                {
                    ApplyBuffToAlly(target);
                    ShowBuffEffect(target.Pos);
                    _nextBuffTime = time + _buffCooldown + _stopDuration;
                }
                _isBuffing = false;
                _isStopped = false;
            });
        }

        private IReadOnlyUnit FindAllyToBuff()
        {
            var allies = GetAllyUnitsInRange();

            foreach (var ally in allies)
            {
                
                if (!HasActiveBuff(ally))
                {
                    return ally;
                }
            }

            return null;
        }

        private bool HasActiveBuff(IReadOnlyUnit ally)
        {
            Unit targetUnit = ally as Unit;
            if (targetUnit == null) return false;

            return _effectSystem.HasActiveBuff(targetUnit);
        }

        private void ApplyBuffToAlly(IReadOnlyUnit ally)
        {

            Unit targetUnit = ally as Unit;
            if (targetUnit == null) return;
            var speedBuff = new SpeedBuff(4f, 1.3f);
            var attackBuff = new AttackRateBuff(4f, 1.2f);

            _effectSystem.AddEffect(targetUnit, speedBuff);
            _effectSystem.AddEffect(targetUnit, attackBuff);
            if (targetUnit.GetBrain() is SecondUnitBrain secondBrain)
            {
                var doubleShotBuff = new DoubleShotBuff(4f);
                _effectSystem.AddEffect(targetUnit, doubleShotBuff);
            }
            if (targetUnit.GetBrain() is ThirdUnitBrain)
            {
                var rangeBuff = new RangeBuff(4f, 1.5f);  
                _effectSystem.AddEffect(targetUnit, rangeBuff);
            }
        }

        private void ShowBuffEffect(Vector2Int position)
        {
            _vfxView?.PlayVFX(position, VFXView.VFXType.BuffApplied);
        }

        private List<IReadOnlyUnit> GetAllyUnitsInRange()
        {
            var alliesInRange = new List<IReadOnlyUnit>();
            var allUnits = runtimeModel.RoUnits;
            float attackRange = unit.Config.AttackRange;
            float attackRangeSqr = attackRange * attackRange;

            foreach (var ally in allUnits)
            {
                
                if (ally != unit && ally.Config.IsPlayerUnit == unit.Config.IsPlayerUnit)
                {
                    float distanceSqr = (ally.Pos - unit.Pos).sqrMagnitude;
                    if (distanceSqr <= attackRangeSqr)
                    {
                        alliesInRange.Add(ally);
                    }
                }
            }

            return alliesInRange;
        }
    }
}
