using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Model;
using Model.Runtime;
using UnityEngine;
using Utilities;
using View;


namespace UnitBrains.Player
{
    public class BufferUnitBrain : BaseUnitBrain
    {
        public override string TargetUnitName => "Buffer";
        private float _buffCooldown = 10f;
        private float _lastBuffTime = -10f;
        private float _buffDuration = 5f;
        private float _buffSpeedModifier = 1.5f;
        private float _duration = 1.0f;

        private RuntimeModel _runtimeModel;
        private VFXView _vfxView;

        public BufferUnitBrain()
        {
            _runtimeModel = ServiceLocator.Get<RuntimeModel>();
            _vfxView = ServiceLocator.Get<VFXView>();
        }

        public override void Update(float deltaTime, float time)
        {
            base.Update(deltaTime, time);
            ApplyBuffIfPossible();
        }

        private void ApplyBuffIfPossible()
        {
            if (Time.time - _lastBuffTime < _buffCooldown) return;

            var alliesInRange = FindAlliesInRange(unit.Pos, unit.Config.AttackRange);
            foreach (var ally in alliesInRange)
            {
                if (!ally.HasBuff("SpeedBuff"))
                {
                    _lastBuffTime = Time.time;
                    CoroutineManager.Instance.StartCoroutine(ApplyBuffWithDelay(ally));
                    break;
                }
            }
        }

        private IEnumerable<Unit> FindAlliesInRange(Vector2Int position, float range)
        {
            return _runtimeModel.RoUnits
                .Where(unit => unit.PlayerId == RuntimeModel.PlayerId &&
                               Vector2Int.Distance(unit.Pos, position) <= range)
                .Cast<Unit>();
        }



        private IEnumerator ApplyBuffWithDelay(Unit ally)
        {
            yield return new WaitForSeconds(0.5f);
            //ally.ApplyBuff(new BuffDebuff(_duration, _buffDuration, _buffSpeedModifier, "SpeedBuff"));
            _vfxView.PlayVFX(ally.Pos, VFXView.VFXType.BuffApplied);
            yield return new WaitForSeconds(0.5f);
        }
    }
}


