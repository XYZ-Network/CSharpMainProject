using Model.Runtime.Projectiles;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnitBrains.Player;
using UnityEngine;
using Utilities;

namespace UnitBrains.Player
{
    public class ThridUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Ironclad Behemoth";
        private float _changeModeTime = 0f;
        private float _ChangeModeCooldown = 2f;
        private bool isMove = true;
        private bool isShooting = false;

        public override void Update(float deltaTime, float time)
        {
            if (isMove) isMove = ChangeMode();
            if (isShooting) isShooting = ChangeMode();
        }
        public override Vector2Int GetNextStep()
        {
            if (HasTargetsInRange() || isShooting)
            {
                return unit.Pos;
            }
            isMove = true;
            return base.GetNextStep();
        }

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            if (isMove) return;
            isShooting = true;
            base.GenerateProjectiles(forTarget, intoList);
        }

        private bool ChangeMode()
        {
            _changeModeTime += Time.deltaTime;
            float t = _changeModeTime / (_ChangeModeCooldown / 10);
            if (t >= 1)
            {
                _changeModeTime = 0;
                return false;
            }
            return true;
        }
    }
}
