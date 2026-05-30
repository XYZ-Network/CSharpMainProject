using System.Collections.Generic;
using Model.Runtime.Projectiles;
using UnityEngine;
using View;

namespace UnitBrains.Player
{
    public class BufferUnitBrain : BaseUnitBrain
    {
        public override string TargetUnitName => "BufferUnit";

        private VFXView _vfxView;
        private float _lastVFXTime = 0f;
        private float _vfxInterval = 2f;

        private VFXView VFXView
        {
            get
            {
                if (_vfxView == null)
                    _vfxView = Object.FindObjectOfType<VFXView>();
                return _vfxView;
            }
        }

        public override void Update(float deltaTime, float time)
        {
            if (unit.IsDead) return;

            // Делаем всех союзников бессмертными
            foreach (var ally in runtimeModel.RoUnits)
            {
                if (ally.Config.IsPlayerUnit == IsPlayerUnitBrain)
                {
                    if (ally is Model.Runtime.Unit unitTarget)
                    {
                        unitTarget.SetInvincible(true);
                    }
                }
            }

            // Показываем визуальный эффект вокруг баффера каждые 2 секунды
            if (time >= _lastVFXTime)
            {
                if (VFXView != null)
                {
                    VFXView.PlayVFX(unit.Pos, VFXView.VFXType.BuffApplied);
                }
                _lastVFXTime = time + _vfxInterval;
            }

            base.Update(deltaTime, time);
        }

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
        }

        protected override List<Vector2Int> SelectTargets()
        {
            return new List<Vector2Int>();
        }
    }
}