using System.Collections.Generic;
using Model;
using UnityEngine;
using Utilities;

namespace UnitBrains.Pathfinding
{
    public class DummyUnitPath : BaseUnitPath
    {
        private const int MaxLength = 100;
        
        public DummyUnitPath(IReadOnlyRuntimeModel runtimeModel, Vector2Int startPoint, Vector2Int endPoint) : base(runtimeModel, startPoint, endPoint)
        {
        }

        protected override void Calculate()
        {
            var currentPoint = startPoint;
            var result = new List<Vector2Int> { startPoint };
            var counter = 0;
            while (currentPoint != endPoint && counter++ < MaxLength)
            {
                var nextStep = CalcNextStepTowards(currentPoint, endPoint);
                var hasLoop = result.Contains(nextStep);
                result.Add(nextStep);
                if (hasLoop)
                    break;
                currentPoint = nextStep;
            }

            path = result.ToArray();
        }
        
        private Vector2Int CalcNextStepTowards(Vector2Int fromPos, Vector2Int toPos)
        {
            var diff = toPos - fromPos;
            var stepDiff = diff.SignOrZero();
            var nextStep = fromPos + stepDiff;

            if (runtimeModel.IsTileWalkable(nextStep))
                return nextStep;

            if (stepDiff.sqrMagnitude > 1)
            {
                var partStep0 = fromPos + new Vector2Int(stepDiff.x, 0);
                if (runtimeModel.IsTileWalkable(partStep0))
                    return partStep0;
                
                var partStep1 = fromPos + new Vector2Int(0, stepDiff.y);
                if (runtimeModel.IsTileWalkable(partStep1))
                    return partStep1;
            }

            var sideStep0 = fromPos + new Vector2Int(stepDiff.y, -stepDiff.x);
            var shiftedStep0 = fromPos + (sideStep0 + stepDiff).SignOrZero();
            if (runtimeModel.IsTileWalkable(shiftedStep0))
                return shiftedStep0;
            
            var sideStep1 = fromPos + new Vector2Int(-stepDiff.y, stepDiff.x);
            var shiftedStep1 = fromPos + (sideStep1 + stepDiff).SignOrZero();
            if (runtimeModel.IsTileWalkable(shiftedStep1))
                return shiftedStep1;
            
            if (runtimeModel.IsTileWalkable(sideStep0))
                return sideStep0;
            
            if (runtimeModel.IsTileWalkable(sideStep1))
                return sideStep1;
            
            return fromPos;
        }
    }
}