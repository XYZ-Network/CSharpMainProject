using BuffSystem;
using Model;
using Model.Runtime.Projectiles;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnitBrains.Pathfinding;
using UnitBrains.Player;
using UnityEngine;
using Utilities;
using static PlasticPipe.Server.MonitorStats;

namespace UnitBrains.Player
{
    public class ThirdUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Ironclad Behemoth";
        private bool _Attacking = false;
        private bool _Moving = false;
        private bool _Pause = false;
        private float _Pause_OneSecond = 1f;
        private float _Pause_FixedTime = 0f;

        protected void Pause()
        {
            _Pause = true;
            _Pause_FixedTime = Time.time;
        }

        public override void Update(float deltaTime, float time)
        {
            if (_Pause)
            {
                if ((Time.time - _Pause_FixedTime) >= _Pause_OneSecond)
                {
                    _Pause = false;
                    //unit._buffManager.AddBuff(this.unit, BuffType.MoveSpeed, 1f, -0.15f); //это я проверял, что юнит может сам себя бафать при опр. логике
                    //unit._buffManager.AddBuff(this.unit, BuffType.AttackSpeed, 1f, -0.05f);
                }
                else
                {
                    return;
                }
            }
            base.Update(deltaTime, time);
        }

        public override Vector2Int GetNextStep()
        {
            if (_Pause || _Attacking)
            {
                return unit.Pos;
            }
            else
            {
                if (HasTargetsInRange())
                {
                    if (_Moving)
                    {
                        Pause();
                        _Moving = false;
                        return unit.Pos;
                    }
                }
                else
                {
                    _Moving = true;
                }
            }
            return base.GetNextStep();
        }

        protected override List<Vector2Int> SelectTargets()
        {
            if (_Pause || _Moving)
            {
                return new();
            }
            else
            {
                if (HasTargetsInRange())
                {
                    if (_Attacking)
                    {
                        return base.SelectTargets();
                    }
                    else
                    {
                        Pause();
                        _Attacking = true;
                    }
                }
                else
                {
                    _Attacking = false;
                }
            }
            return new();
        }
    }
}