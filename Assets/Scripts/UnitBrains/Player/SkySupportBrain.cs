using Buffs;
using Buffs.Buffs;
using Model.Runtime;
using Model.Runtime.Projectiles;
using Model.Runtime.ReadOnly;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnitBrains;
using UnitBrains.Player;
using UnityEngine;
using Utilities;
using View;

namespace UnitBrains.Player
{
    public class SkySupportBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Sky Support";

        private Vector2Int _prevPos;

        private AStarUnitPath _activePath = null;

        private float _buffCoolDown = 0.3f;

        private float _buffTimer = 0;
        
        private BuffSystem _buffSystem => ServiceLocator.Get<BuffSystem>();
        public override Vector2Int GetNextStep()
        {
            if (_prevPos == null)
            {
                _prevPos = unit.Pos;
            }


            var closestAlly = FindClosestAlly(5);
            Vector2Int target;

            if (closestAlly.Config != unit.Config)
            {
                target = new Vector2Int(closestAlly.Pos.x, closestAlly.Pos.y - 1);
            }
            else
            {
                target = _unitCoordinator.GetTargetPosRecommendation();
            }

            _activePath = new AStarUnitPath(runtimeModel, unit.Pos, target, _prevPos);
            _prevPos = _activePath.GetPrevPos();
            Vector2Int nextStep = _activePath.GetNextStepFrom(unit.Pos);

            return nextStep;
        }
        public override void Update(float deltaTime, float time)
        {
            _buffTimer += Time.deltaTime;
 
            if (_buffTimer >= _buffCoolDown)
            {
                _buffTimer = 0;
                BuffAllyInRadius(5);
            }
        }


        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
           
        }
        private IReadOnlyUnit FindClosestAlly(int radius)
        {
            IReadOnlyUnit closestAlly = unit;
            float minDistance = radius;
            foreach (var ally in runtimeModel.RoPlayerUnits)
            {
                if (ally == unit)
                    continue;

                if (Vector2Int.Distance(ally.Pos, unit.Pos) <= minDistance)
                {
                    closestAlly = ally;
                    minDistance = Vector2Int.Distance(unit.Pos, closestAlly.Pos);
                }
            }
            return closestAlly;
        }

        private void BuffAllyInRadius (float radius)
        {
            foreach (var ally in runtimeModel.RoPlayerUnits)
            {
                if (ally.Config == unit.Config)
                    continue;
                if (Vector2Int.Distance(ally.Pos, unit.Pos) <= radius)
                {
                    foreach (Buff<BaseUnitBrain> bufftype in _buffSystem.availableBuffs)
                    {
                        _buffSystem.ApplyBuff(ally, bufftype);

                    }
                }
            }
        }
    }


}
