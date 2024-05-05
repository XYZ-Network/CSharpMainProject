using System;
using System.Collections.Generic;
using Model.Runtime;
using Model.Runtime.Projectiles;
using Model.Runtime.ReadOnly;
using UnitBrains.Player;
using UnityEngine;
using View;

namespace AssemblyCSharp.Assets.Scripts.UnitBrains.Player
{
    public class DublUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Snake Commando"; //задаем имя unit brain
        private float buffTimer = 0;
        private float buffInterval = 5.0f;
        BuffSystem buffSystem = new BuffSystem();
        Buff moveBuff = new Buff(2, 1, 0); 
        Buff attackBuff = new Buff(2, 0, 1);
        private IEnumerable<IReadOnlyUnit> RoPlayerUnits;
        private VFXView _vfxView;

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
        }

        public override void Update(float deltaTime, float time)
        {
            base.Update(deltaTime, time); 

            buffTimer += deltaTime;
            if (buffTimer >= buffInterval)
            {
                ApplyBuff(RoPlayerUnits,moveBuff);
                buffTimer = 0;
            }
        }

        public void ApplyBuff(IEnumerable<IReadOnlyUnit> RoPlayerUnits, Buff buffToApply)
        {
            foreach (Unit myUnit in RoPlayerUnits)
            {
                if (IsTargetInRange(myUnit.Pos))
                {
                    if (!buffSystem.HasBuff(myUnit))
                    {
                        buffSystem.AddBuff(myUnit,buffToApply);
                        _vfxView.PlayVFX(myUnit.Pos, VFXView.VFXType.BuffApplied);
                    }
                }
            }
        }

    }

}
