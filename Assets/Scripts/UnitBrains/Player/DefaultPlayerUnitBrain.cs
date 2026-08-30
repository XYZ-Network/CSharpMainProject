//using Assets.Scripts.UnitBrains;
using Model;
using Model.Runtime.Projectiles;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;
using static UnityEngine.GraphicsBuffer;

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
        //public override Vector2Int GetNextStep()
        //{
        //    var coordinator = Coordinator.GetInstance();
        //    var diff = coordinator.GetRecomendedTarget() - unit.Pos;
        //    var distance = diff.sqrMagnitude;
        //    if (distance <= unit.Config.AttackRange * 2)
        //    {
        //        var activePath = new Pathfinding.AdvancedUnitPath(runtimeModel, unit.Pos, coordinator.GetRecomendedTarget());
        //        return activePath.GetNextStepFrom(unit.Pos);
        //    }


        //    return base.GetNextStep();
        //}
        private int CompareByDistanceToOwnBase(Vector2Int a, Vector2Int b)
        {
            var distanceA = DistanceToOwnBase(a);
            var distanceB = DistanceToOwnBase(b);
            return distanceA.CompareTo(distanceB);
        }
    }
}