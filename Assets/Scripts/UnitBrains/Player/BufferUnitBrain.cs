using System.Collections.Generic;
using System.Linq;
using Model;
using Model.BuffSystem;
using Model.BuffSystem.Buffs;
using Model.Runtime.Projectiles;
using Model.Runtime.ReadOnly;
using UnityEngine;
using Utilities;
using View;

namespace UnitBrains.Player
{
    public class BufferUnitBrain : BaseUnitBrain
    {
        public override string TargetUnitName => "Buffer";
        public override bool IsPlayerUnitBrain => true;

        private const float BuffCooldown = 5f;
        private const float BuffDuration = 3f;
        private const float StopDuration = 0.5f;

        private float _lastBuffTime;
        private float _stopEndTime;
        private bool _isStopped;

        public override void Update(float deltaTime, float time)
        {
            if (unit.IsDead) return;

            if (_isStopped && time >= _stopEndTime)
            {
                _isStopped = false;
            }

            if (_isStopped) return;

            if (time >= _lastBuffTime + BuffCooldown)
            {
                TryApplyBuff(time);
            }
        }

        protected override List<Vector2Int> SelectTargets()
        {
            return new List<Vector2Int>();
        }

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
        }

        private void TryApplyBuff(float currentTime)
        {
            var allies = GetAlliesInRange();
            if (allies.Count == 0) return;

            var target = allies.FirstOrDefault(a => !HasBuff(a));
            if (target == null) return;

            _isStopped = true;
            _stopEndTime = currentTime + StopDuration * 2;

            if (currentTime >= _lastBuffTime + BuffCooldown + StopDuration)
            {
                ApplyBuffTo(target);
                _lastBuffTime = currentTime;

                var vfx = ServiceLocator.Get<VFXView>();
                vfx?.PlayVFX(unit.Pos, VFXView.VFXType.BuffApplied);
            }
        }

        private List<IReadOnlyUnit> GetAlliesInRange()
        {
            return runtimeModel.RoUnits
                .Where(u => u.Config.IsPlayerUnit == IsPlayerUnitBrain).ToList();
        }

        private bool HasBuff(IReadOnlyUnit unit)
        {
            var buffSystem = ServiceLocator.Get<BuffSystemManager>();
            return buffSystem?.HasBuffs(unit) ?? false;
        }

        private void ApplyBuffTo(IReadOnlyUnit target)
        {
            var buffSystem = ServiceLocator.Get<BuffSystemManager>();
            if (buffSystem == null) return;

            switch (target.Config.Name)
            {
                case "Cobra Commando":
                    buffSystem.ApplyBuff(target, new DoubleShotBuff(BuffDuration));
                    break;
                case "Ironclad Behemoth":
                    buffSystem.ApplyBuff(target, new IncreasedRangeBuff(BuffDuration));
                    break;
            }
        }

        public override Vector2Int GetNextStep()
        {
            if (_isStopped)
                return unit.Pos;

            var enemyBasePos = runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId];
            var path = new AStarUnitPath(runtimeModel, unit.Pos, enemyBasePos);
            return path.GetNextStepFrom(unit.Pos);
        }
    }
}