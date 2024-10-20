using System;
using System.Collections.Generic;
using Assets.Scripts.Pathfinding;
using Assets.Scripts.UnitBrains;
using Model;
using Model.Runtime.Projectiles;
using UnityEngine;

namespace UnitBrains.Player
{
    public class DefaultPlayerUnitBrain : BaseUnitBrain
    {
        protected float DistanceToOwnBase(Vector2Int fromPos) =>
            Vector2Int.Distance(fromPos, runtimeModel.RoMap.Bases[RuntimeModel.PlayerId]);

        protected void SortByDistanceToOwnBase(List<Vector2Int> list)
        {
            list.Sort(CompareByDistanceToOwnBase);
        }
        
        private int CompareByDistanceToOwnBase(Vector2Int a, Vector2Int b)
        {
            var distanceA = DistanceToOwnBase(a);
            var distanceB = DistanceToOwnBase(b);
            return distanceA.CompareTo(distanceB);
        }

        public override Vector2Int GetNextStep()
        {

            if (HasTargetsInRange())
                return unit.Pos;

            SingletonForUnitCoordination coordinator = SingletonForUnitCoordination.GetInstance();
            MyUnitPath _activePath;

            Vector2Int preferredTarget = coordinator.PreferredTargetForPlayer;
            Vector2Int preferredPos = coordinator.PreferredPosForPlayer;
    
            if (coordinator.DoPreferredStep(unit, preferredTarget) && preferredTarget!=Vector2Int.zero)
            {
                _activePath = new MyUnitPath(runtimeModel, unit.Pos, preferredTarget, IsPlayerUnitBrain);
                Debug.Log(1);
                Debug.Log(preferredTarget);
                return _activePath.GetNextStepFrom(unit.Pos);
            }
            Debug.Log(2);
            Debug.Log(preferredPos);
            _activePath = new MyUnitPath(runtimeModel, unit.Pos, preferredPos, IsPlayerUnitBrain);
            return _activePath.GetNextStepFrom(unit.Pos);
        }
        

    }
}