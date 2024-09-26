using Assets.Scripts.Utilities.Buffs;
using Model.Runtime.Projectiles;
using Model.Runtime.ReadOnly;
using System.Collections.Generic;
using System.Linq;
using UnitBrains.Player;
using UnityEngine;
using Utilities;
using View;

namespace Assets.Scripts.UnitBrains.Player
{
    public class BufferUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Buffalo";
        private float BuffDelay = 0.5f;
        private float MoveDelay = 0.0f;

        public BufferUnitBrain()
        {
        }

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            return;
        }

        public override void Update(float deltaTime, float time)
        {
            BuffDelay -= deltaTime;
            MoveDelay -= deltaTime;

            if (BuffDelay > 0)
            {
                return;
            }

            IReadOnlyUnit[] allFriendlyTargets = runtimeModel.RoUnits
                    .Where(u => u.Config.IsPlayerUnit == IsPlayerUnitBrain && u.Pos != unit.Pos).ToArray();

            if (allFriendlyTargets.Length > 0)
            {
                IReadOnlyUnit unit = allFriendlyTargets[Random.Range(0, allFriendlyTargets.Length - 1)];
                ServiceLocator.Get<BuffController>().addEffect((Model.Runtime.Unit)unit, new RapidFireBuff((Model.Runtime.Unit)unit));
                ServiceLocator.Get<VFXView>().PlayVFX(unit.Pos, VFXView.VFXType.BuffApplied);

                BuffDelay = 0.5f;
                MoveDelay = 0.5f;
            }
        }

        public override Vector2Int GetNextStep()
        {
            if (MoveDelay > 0.5f)
                return unit.Pos;

            return base.GetNextStep();
        }
    }
}