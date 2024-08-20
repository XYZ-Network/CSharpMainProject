using System;
using System.Collections.Generic;
using System.Linq;
using Codice.CM.Triggers;
using Model;
using Model.Config;
using Model.Runtime;
using Model.Runtime.Projectiles;
using UnitBrains.Pathfinding;
using UnityEngine;
using Utilities;
using View;


namespace UnitBrains.Player
{
    public class FourthUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Buffer";
        private float _cooldownTime = 0f;
        private float timeBetweenBuffs = 0.5f;
        private bool firstBuffStart = true;

        VFXView _vfx = ServiceLocator.Get<VFXView>();
        BuffSystem _buffSystem = ServiceLocator.Get<BuffSystem>();
       
        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {



        }

        public override void Update(float deltaTime, float time)
        {

            _cooldownTime += Time.deltaTime;
            if(_cooldownTime > timeBetweenBuffs || firstBuffStart)
            {
                if (firstBuffStart)
                    firstBuffStart = false;
                foreach (Unit target in runtimeModel.RoPlayerUnits)
                {
                    if (unit == target)
                        continue;
                    if(_buffSystem.Buffs.ContainsKey(target))
                        continue;
                    if(IsTargetInRange(target.Pos))
                    {
                        _buffSystem.AddBuff(target, CreateBuff(target.Config));
                        _vfx.PlayVFX(target.Pos, VFXView.VFXType.BuffApplied);
                        break;
                    }
                    
                }
                _cooldownTime = 0;
            }

        }
        public IBuff<Unit> CreateBuff(UnitConfig unitConfig)
        {
            switch (unitConfig.Type)
            {
                case "DefaultUnit":
                    return new AttackSpeedBuff();
                case "SecondUnit":
                    return new DoubleAttackBuff();
                case "ThirdUnit":
                    return new AttackRangeBuff();
                case "SupportUnit":
                    return new SpeedBuff();
                default:
                    throw new ArgumentException($"Unknown unit type: {unitConfig.Type}");
            }
        }

    }
}
