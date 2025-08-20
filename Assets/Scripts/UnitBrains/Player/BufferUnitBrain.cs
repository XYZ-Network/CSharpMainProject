using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Collections.Generic;
using Model.Runtime.Projectiles;
using Model.Runtime.ReadOnly;
using UnityEngine;
using Assets.Scripts.UnitBrains.Player;
using Utilities;
using View;

namespace UnitBrains.Player
{
    public class BufferUnitBrain : BaseUnitBrain
    {
        private float _buffCooldown = 5f;   // ??? ? 5 ??????
        private float _buffTimer = 0f;
        private float _stopTime = 0f;
        private bool _isStopped = false;

        public override string TargetUnitName => "BufferUnit";
        public override bool IsPlayerUnitBrain => true;

        public override void Update(float deltaTime, float time)
        {
            // ????????? "?????????"
            if (_isStopped)
            {
                _stopTime -= deltaTime;
                if (_stopTime <= 0f) _isStopped = false;
                return;
            }

            _buffTimer -= deltaTime;
            if (_buffTimer <= 0f)
            {
                TryApplyBuff();
                _buffTimer = _buffCooldown;
            }
        }

        private void TryApplyBuff()
        {
            var allies = GetUnitsInRadius(unit.Config.AttackRange, enemies: false);
            foreach (var ally in allies)
            {
                // ????????: ???? ?? ??? ????
                var buffSystem = ServiceLocator.Get<BuffSystem>();
                if (!buffSystem.HasBuff(ally))
                {
                    // ??????? ????
                    var buff = new Buff(duration: 5f, moveMod: 1.5f);
                    buffSystem.AddBuff((Model.Runtime.Unit)ally, buff);

                    // ?????????? VFX
                    var vfx = ServiceLocator.Get<VFXView>();
                    vfx.PlayVFX(ally.Pos, VFXView.VFXType.BuffApplied);

                    // ????????? ?? 0.5 ??? ?? ?????????? ????????
                    _isStopped = true;
                    _stopTime = 0.5f;
                    break; // ??????????? ?????? ?? ??????
                }
            }
        }

        protected override List<Vector2Int> SelectTargets()
        {
            // ?????? ?? ???????, ??????? ????? ???
            return new List<Vector2Int>();
        }

        public override Vector2Int GetNextStep()
        {
            if (_isStopped)
                return unit.Pos; // ????? ?? ?????

            return base.GetNextStep(); // ??????? ???????? ? ????
        }
    }
}
