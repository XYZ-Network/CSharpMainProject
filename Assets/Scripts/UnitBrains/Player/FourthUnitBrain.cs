using System;
using System.Collections.Generic;
using System.Linq;
using Model.Runtime;
using Model.Runtime.ReadOnly;
using UnityEngine;
using Utilities;
using View;

namespace UnitBrains.Player
{
    public class FourthUnitBrain : DefaultPlayerUnitBrain
    {
        private BuffSystem _buffSystem => ServiceLocator.Get<BuffSystem>();
        private VFXView _vfxView;
        private float _timeSinceLastBuff = 0f;
        private readonly float _BUFF_OVERCHARGE_TIME = 4f; // seconds

        public override string TargetUnitName => "Hippomagus";

        public FourthUnitBrain()
        {
            _vfxView = ServiceLocator.Get<VFXView>();
        }

        public override void Update(float deltaTime, float time)
        {
            if (time - _timeSinceLastBuff >= _BUFF_OVERCHARGE_TIME)
            {
                _timeSinceLastBuff = time;
                this.activateBuff();
            }
        }

        public override Vector2Int GetNextStep()
        {
            // if there was a buff attempt in the recent half a second
            if (Math.Abs(Time.time - _timeSinceLastBuff) <= 0.5f)
            {
                return unit.Pos;
            }

            return base.GetNextStep();
        }

        private void activateBuff()
        {
            List<IReadOnlyUnit> confederates = this.GetUnitsInRadius(10, true).Where((unit) => !Vector2Int.Equals(unit.Pos, this.unit.Pos)).ToList();
            if (confederates.Count == 0)
            {
                Debug.Log("No units in sight, skip buff");
                return;
            }
            
            Unit unitForBuff = (Unit)confederates[0];
            var isUnitHasBuff = _buffSystem.getBuffs(unitForBuff).Length > 0;
            if (isUnitHasBuff)
            {
                Debug.Log("Unit has buff, skip buff");
                return;
            }

            AbstractBuff buff = new UpAttackSpeedBuff(unitForBuff);
            Debug.Log("Set buff " + buff.Type);
            _vfxView.PlayVFX(unitForBuff.Pos, VFXView.VFXType.BuffApplied);
            _buffSystem.setBuff(new UpAttackSpeedBuff(unitForBuff));
        }

        protected override List<Vector2Int> SelectTargets()
        {
            return new List<Vector2Int>();
        }
    }
}