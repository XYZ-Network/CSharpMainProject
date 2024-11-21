using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.UnitBrains.Buffs;
using Model;
using Model.Runtime;
using Model.Runtime.ReadOnly;
using UnitBrains.Player;
using UnityEngine;
using Utilities;
using View;

namespace Assets.Scripts.UnitBrains.Player
{
    public class BuffUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "PlayerUnit4";
        private bool _freezing = false;
        private readonly float _freezingTime = 0.5f;
        private float _freezingTimer = 0f;
        private float buffCooldown = 2.0f;
        private float buffCooldownTimer = 0;

        private BuffController buffController;
        private VFXView _vfxView;

        public BuffUnitBrain()
        {
            buffController = ServiceLocator.Get<BuffController>();
            _vfxView = ServiceLocator.Get<VFXView>();
        }

        public override Vector2Int GetNextStep()
        {
            return !_freezing ? base.GetNextStep() : unit.Pos;
        }

        public override void Update(float deltaTime, float time)
        {
            if (HasAlliesInRange() && buffCooldownTimer <= 0)
            {
                AddBuffEffect();
            }
            else if (!_freezing)
            {
                buffCooldownTimer -= deltaTime;
            }

            _freezingTimer -= deltaTime;
            _freezing = _freezingTimer > 0;

            base.Update(deltaTime, time);
        }

        private void AddBuffEffect()
        {
            _freezingTimer = _freezingTime;
            buffCooldownTimer = buffCooldown;

            foreach (var target in GetAlliesInRange())
            {
                if (buffController.GetBuffModifierForUnit(target as Model.Runtime.Unit, Buff.BuffType.MoveSpeed) <= 1.0f)
                {
                    var buff = new Buff("SpeedBuff", Buff.BuffType.MoveSpeed, 1.5f, 5f);
                    buffController.AddBuffToUnit(target as Model.Runtime.Unit, buff);

                    if (_vfxView != null)
                    {
                        _vfxView.PlayVFX(target.Pos, VFXView.VFXType.BuffApplied);
                    }
                }
            }

            _freezingTimer += _freezingTime;
        }

        private bool HasAlliesInRange()
        {
            var attackRangeSqr = unit.Config.AttackRange * unit.Config.AttackRange;
            return runtimeModel.RoPlayerUnits
                .Any(possibleTarget => (possibleTarget.Pos - unit.Pos).sqrMagnitude < attackRangeSqr);
        }

        private IEnumerable<IReadOnlyUnit> GetAlliesInRange()
        {
            var attackRangeSqr = unit.Config.AttackRange * unit.Config.AttackRange;
            return runtimeModel.RoPlayerUnits
                .Where(possibleTarget => (possibleTarget.Pos - unit.Pos).sqrMagnitude < attackRangeSqr);
        }
    }
}
