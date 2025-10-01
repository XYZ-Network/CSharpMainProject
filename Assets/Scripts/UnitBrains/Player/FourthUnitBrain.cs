using Assets.Scripts.Model.Runtime;
using Model.Runtime.ReadOnly;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities;
using View;
using Model.Runtime;
using Controller;

namespace UnitBrains.Player
{
    public class FourthUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "BuffCar";

        private const float BuffInterval = 5f;
        private const float BuffPauseDuration = 0.5f;
        private float _lastBuffTime = -10f;
        private float _buffStartTime = -10f;
        private bool _isBuffing = false;
        private BuffsController _buffsController;

        public override void Update(float deltaTime, float time)
        {
            base.Update(deltaTime, time);

            if (_buffsController == null)
            {
                _buffsController = ServiceLocator.Get<BuffsController>();
            }

            if (_isBuffing)
            {
                if (time >= _buffStartTime + BuffPauseDuration * 2)
                {
                    _isBuffing = false;
                }
                return;
            }

            if (time - _lastBuffTime >= BuffInterval)
            {
                var ally = FindAllyToBuff();
                if (ally != null)
                {
                    _isBuffing = true;
                    _buffStartTime = time;
                    _lastBuffTime = time;

                    ApplyBuff(ally);
                    ShowVFX();
                }
            }
        }

        public override Vector2Int GetNextStep()
        {
            if (_isBuffing)
                return unit.Pos;

            return base.GetNextStep();
        }

        protected override List<Vector2Int> SelectTargets()
        {
            return new List<Vector2Int>();
        }

        private IReadOnlyUnit FindAllyToBuff()
        {
            var allies = GetUnitsInRadius(this.unit.AttackRange, false);

            return allies.FirstOrDefault(ally =>
            {
                if (ally is Unit runtimeUnit)
                    return !_buffsController.HasBuff<BuffCarBuff>(runtimeUnit);
                return false;
            });
        }

        private void ApplyBuff(IReadOnlyUnit targetUnit)
        {
            if (targetUnit is Model.Runtime.Unit unitToBuff)
            {
                var buff = new BuffCarBuff(10f, 1.5f);
                _buffsController.AddBuff(unitToBuff, buff);
            }
        }

        private void ShowVFX()
        {
            var vfxView = ServiceLocator.Get<VFXView>();
            if (vfxView != null)
            {
                vfxView.PlayVFX(unit.Pos, VFXView.VFXType.BuffApplied);
            }
        }
    }
}