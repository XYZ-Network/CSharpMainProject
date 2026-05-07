using BuffSystem;
using Model;
using Model.Runtime.Projectiles;
using Model.Runtime.ReadOnly;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities;
using View;
using static UnityEngine.GraphicsBuffer;
using Model.Runtime;

namespace UnitBrains.Player
{
    public class BufferUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Buffer";
        private bool _Pause = false;
        private float _Pause_HalfASecond = 0.5f;
        private float _Pause_FixedTime = 0f;
        private VFXView _vfxView = ServiceLocator.Get<VFXView>();
        private IBuffSystem _buffSystem = ServiceLocator.Get<IBuffSystem>();

        protected void Pause()
        {
            _Pause = true;
            _Pause_FixedTime = Time.time;
        }

        public override void Update(float deltaTime, float time)
        {
            if (_Pause)
            {
                if ((Time.time - _Pause_FixedTime) >= _Pause_HalfASecond)
                {
                    _Pause = false;
                }
                else
                {
                    return;
                }
            }
            var targetToBuff = FindUnitToBuff();
            if (targetToBuff != null && _Pause != true)
            {
                if (targetToBuff.Config.Name == "Cobra Commando")
                {
                    _buffSystem.AddBuff(targetToBuff, BuffType.AttackCount, 2f, 1f);
                    _buffSystem.AddBuff(targetToBuff, BuffType.MoveSpeed, 2f, -0.2f);
                    _vfxView.PlayVFX(targetToBuff.Pos, VFXView.VFXType.BuffApplied);
                }
                else if (targetToBuff.Config.Name == "Ironclad Behemoth")
                {
                    _buffSystem.AddBuff(targetToBuff, BuffType.AttackRange, 20f, 3.5f);
                    _vfxView.PlayVFX(targetToBuff.Pos, VFXView.VFXType.BuffApplied);
                }
                else if(targetToBuff.Config.Name == "Sky Serpent")
                {
                    _buffSystem.AddBuff(targetToBuff, BuffType.MoveSpeed, 5f, -0.15f);
                    _buffSystem.AddBuff(targetToBuff, BuffType.AttackSpeed, 5f, -0.15f);
                    _vfxView.PlayVFX(targetToBuff.Pos, VFXView.VFXType.BuffApplied);
                }

                Pause();
            }
        }

        public override Vector2Int GetNextStep()
        {
            if (_Pause)
            {
                return unit.Pos;
            }
            return base.GetNextStep();
        }

        public Unit FindUnitToBuff()
        {
            var alliedUnits = GetUnitsInRadius(this.unit.AttackRange, true);
            if (alliedUnits.Count > 0)
            {
                var sortedUnits = alliedUnits
                .OrderBy(u => Vector2Int.Distance(u.Pos, this.unit.Pos))
                .ToList();

                foreach (var unit in sortedUnits)
                {
                    if (unit == this.unit || unit.Config.Name == "Buffer") //чтобы сам себя не выбирал в цель для бафа и не выбирал других баферов
                    {
                        continue;
                    }
                    //var target = this.unit.FindUnitByPosition(unit.Pos, isPlayerUnit); //тут работает прямое приведение и оно быстрее, поэтому убрал этот метод
                    if (_buffSystem.HasAnyBuff((Unit)unit) == false)
                        return (Unit)unit;
                }
            }
            return null;
        }

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            //AddProjectileToList(CreateProjectile(forTarget), intoList); убрали возможность атаковать
        }
    }
}