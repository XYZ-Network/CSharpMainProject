using Model.Runtime.Projectiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitBrains.Player;
using UnityEngine;
using System.Collections;
using View;
using Utilities;

namespace Assets.Scripts.UnitBrains.Player
{
    public class BufferUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Buffer Muffin";

        private const float Cooldown = 1f;
        private const float StopTime = 0f;
        private float _timer = 0;

        private bool _isBuffing = false;
        private VFXView _view => ServiceLocator.Get<VFXView>();

        private float beforeStopTime = (Cooldown - StopTime) / Cooldown;
        private float afterStopTime = StopTime / Cooldown;
        public override void Update(float deltaTime, float time)
        {
            _timer += Time.deltaTime;
            float t = _timer / (Cooldown / 10);
            if(t >= beforeStopTime || t <= afterStopTime)
            {
                _isBuffing = true;
            }
            else
            {
                _isBuffing = false;
            }

            if (t >= 1)
            {
                BuffNearUnit();
                _timer = 0;
            }
        }
        private void BuffNearUnit()
        {
            foreach(var unit in GetUnitsInRadius(unit.Config.AttackRange, IsPlayerUnitBrain))
            {
                if (effectSystem.EffectStatus.ContainsKey(unit))
                    continue;

                effectSystem.AddEffect(unit, StatusType.Buff);
                Debug.Log("buff");
                _view.PlayVFX(unit.Pos, VFXView.VFXType.BuffApplied);
            }
        }
        protected override List<Vector2Int> SelectTargets()
        {
            return new List<Vector2Int>();
        }
        public override Vector2Int GetNextStep()
        {
            if (_isBuffing)
                return unit.Pos;
            return base.GetNextStep();
        }
    }
}
