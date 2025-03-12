using System;
using System.Collections.Generic;
using System.Linq;
using Model;
using Model.Runtime.Projectiles;
using UnitBrains.Pathfinding;
using UnityEngine;
using static Utilities.Extensions;

namespace UnitBrains.Player
{
    public class ThirdUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Ironclad Behemoth";

        public enum Mod 
        {
            Attack,
            Move,
        }
        
        private Mod _currentMode = Mod.Move;

        private long _timeToBlock;

        public override Vector2Int GetNextStep()
        {
            if (HasTargetsInRange())
                return unit.Pos;

            SwitchMode(Mod.Move);
            if (!CheckCanDoAny())
            {
                return unit.Pos;
            }
            
            var target = runtimeModel.RoMap.Bases[
                IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId];

            _activePath = new DummyUnitPath(runtimeModel, unit.Pos, target);
            return _activePath.GetNextStepFrom(unit.Pos);
        }

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            SwitchMode(Mod.Attack);
            if (!CheckCanDoAny())
            {
                return;
            }
            base.GenerateProjectiles(forTarget, intoList);
        }

        private void SwitchMode(Mod newMode)
        {
            if (_currentMode == newMode)
            {
                return;
            }
            if (_currentMode == Mod.Attack)
            {
                _currentMode = Mod.Move;
            }
            else if (_currentMode == Mod.Move)
            {
                _currentMode = Mod.Attack;
            }
            Debug.Log($"{_currentMode}");

            _timeToBlock = DateTimeOffset.Now.ToUnixTimeSeconds() + 1;
        }

        private bool CheckCanDoAny()
        {
            return DateTimeOffset.Now.ToUnixTimeSeconds() > _timeToBlock;
        }
    }
}