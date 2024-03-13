using Model;
using Model.Runtime.Projectiles;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using Utilities;
using UnityEditor;

namespace UnitBrains.Player
{
    public enum UnitStatus
    {
        Attack,
        Move
    }
    public class ThirdUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Ironclad Behemoth";
        private float _switchTime = 1f;
        private float _cooldownTime = 0f;
        private bool _switchMode;
        private UnitStatus _status;

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
         
            var projectile = CreateProjectile(forTarget);
            AddProjectileToList(projectile, intoList);

        }

        public override Vector2Int GetNextStep()
        {
            Vector2Int position = base.GetNextStep();

            if (position == unit.Pos)
            {
                if (_status == UnitStatus.Move)
                {
                    _switchMode = true;
                }
                _status = UnitStatus.Attack;
            }
            else
            {
                if (_status == UnitStatus.Attack)
                {
                    _switchMode = true;
                }
                _status = UnitStatus.Move;
            }

            if (_switchMode)
            {
                return unit.Pos;
            }
            else
            {
                return position;
            }
        }

        protected override List<Vector2Int> SelectTargets()
        {
            if (_switchMode)
            {
                return new List<Vector2Int>();
            }
            if (_status == UnitStatus.Attack)
            {
                return base.SelectTargets();
            }
            return new List<Vector2Int>();
        }

        public override void Update(float deltaTime, float time)
        {
            if (_switchMode)
            {
                _cooldownTime += Time.deltaTime;
                float t = _cooldownTime / (_switchTime / 10);
                if (t >= 1)
                {
                    _cooldownTime = 0;
                    _switchMode = false;
                }
            }
        }
    }
}
