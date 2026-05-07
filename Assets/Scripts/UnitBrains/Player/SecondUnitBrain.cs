using Model;
using Model.Runtime.Projectiles;
using System;
using System.Collections.Generic;
using System.Linq;
using UnitBrains.Pathfinding;
using UnityEngine;
using Utilities;

namespace UnitBrains.Player
{
    public class SecondUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Cobra Commando";

        private const int MaxTargetsToConsider = 3;

        private static int _unitCounter = 0;
        private int _unitIndex = _unitCounter++;

        private readonly List<Vector2Int> _targetsToMove = new();

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            var projectile = CreateProjectile(forTarget);
            AddProjectileToList(projectile, intoList);
        }

        public override Vector2Int GetNextStep()
        {
            if (_targetsToMove.Count == 0)
                return unit.Pos;

            Vector2Int target = _targetsToMove[0];

            if (IsTargetInRange(target))
                return unit.Pos;

            return unit.Pos.CalcNextStepTowards(target);
        }

        protected override List<Vector2Int> SelectTargets()
        {
            List<Vector2Int> result = new();
            _targetsToMove.Clear();

            List<Vector2Int> allTargets = GetAllTargets().ToList();

            Vector2Int enemyBase = runtimeModel.RoMap.Bases[
                IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId
            ];

            allTargets.Remove(enemyBase);

            if (allTargets.Count == 0)
            {
                _targetsToMove.Add(enemyBase);

                if (IsTargetInRange(enemyBase))
                    result.Add(enemyBase);

                return result;
            }

            SortByDistanceToOwnBase(allTargets);

            int targetsCount = Math.Min(MaxTargetsToConsider, allTargets.Count);
            int targetIndex = _unitIndex % targetsCount;

            Vector2Int selectedTarget = allTargets[targetIndex];

            _targetsToMove.Add(selectedTarget);

            if (IsTargetInRange(selectedTarget))
                result.Add(selectedTarget);

            return result;
        }
    }
}