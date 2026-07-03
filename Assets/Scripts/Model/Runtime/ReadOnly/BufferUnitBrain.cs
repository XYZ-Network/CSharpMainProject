using Model;
using Model.Runtime.ReadOnly;
using System.Collections.Generic;
using Model.Runtime.Projectiles;
using UnitBrains.Player;
using UnityEngine;
using Utilities;
using View;

namespace UnitBrains.Player
{
    public class BufferUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Buffer Unit";
        private enum BufferState
        {
            Moving,
            PreBuffPause,   
            Buffing,        
            PostBuffPause   
        }

        private BufferState _state = BufferState.Moving;
        private float _pauseTimer = 0f;
        private const float PauseDuration = 0.5f;  
        private const float BuffCooldown = 3f;     
        private float _buffCooldownTimer = 0f;

        private IReadOnlyUnit _buffTarget = null;  
        private VFXView _vfxView;

        public override void Update(float deltaTime, float time)
        {
            if (_vfxView == null)
                _vfxView = ServiceLocator.Get<VFXView>();

            if (_buffCooldownTimer > 0f)
                _buffCooldownTimer -= deltaTime;

            switch (_state)
            {
                case BufferState.Moving:

                    if (_buffCooldownTimer <= 0f)
                    {
                        _buffTarget = FindBuffTarget();
                        if (_buffTarget != null)
                        {
                            _state = BufferState.PreBuffPause;
                            _pauseTimer = 0f;
                        }
                    }
                    break;

                case BufferState.PreBuffPause:
   
                    _pauseTimer += deltaTime;
                    if (_pauseTimer >= PauseDuration)
                    {
                        _state = BufferState.Buffing;
                    }
                    break;

                case BufferState.Buffing:
                    if (_buffTarget != null)
                    {
                        var targetUnit = _buffTarget as Model.Runtime.Unit;
                        if (targetUnit != null)
                        {
                            var buff = ChooseBuff(targetUnit);
                            ServiceLocator.Get<BuffService>().Apply(buff, targetUnit, 5f);
                            _vfxView.PlayVFX(_buffTarget.Pos, VFXView.VFXType.BuffApplied);
                        }
                    }
                    _buffTarget = null;
                    _buffCooldownTimer = BuffCooldown;
                    _state = BufferState.PostBuffPause;
                    _pauseTimer = 0f;
                    break;

                case BufferState.PostBuffPause:
                    _pauseTimer += deltaTime;
                    if (_pauseTimer >= PauseDuration)
                    {
                        _state = BufferState.Moving;
                    }
                    break;
            }
        }

        public override Vector2Int GetNextStep()
        {
            if (_state != BufferState.Moving)
                return unit.Pos;

            return base.GetNextStep();
        }

        protected override List<Vector2Int> SelectTargets()
        {
            return new List<Vector2Int>();
        }

        private IReadOnlyUnit FindBuffTarget()
        {
            var allies = GetUnitsInRadius(unit.Config.AttackRange, enemies: false);
            foreach (var ally in allies)
            {
                var allyUnit = ally as Model.Runtime.Unit;
                if (allyUnit == null) continue;

                var buff = ChooseBuff(allyUnit);
                if (buff.CanApply(allyUnit))
                    return ally;
            }
            return null;
        }

        private BuffBase ChooseBuff(Model.Runtime.Unit unit)
        {
            if (unit.Config.Name == "Cobra Commando")
                return new DoubleShootBuff();
            if (unit.Config.Name == "Ironclad Behemoth")
                return new AttackRangeBuff();
            return new SpeedBuff();
        }
    }
}