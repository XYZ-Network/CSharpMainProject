using Model;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnitBrains.Pathfinding
{
    public abstract class BaseUnitPath
    {
        public Vector2Int StartPoint => startPoint;
        public Vector2Int EndPoint => endPoint;
        
        protected readonly IReadOnlyRuntimeModel runtimeModel;
        protected readonly Vector2Int startPoint;
        protected readonly Vector2Int endPoint;
        protected Vector2Int[] path = null;

        protected abstract void Calculate();
        
        public IEnumerable<Vector2Int> GetPath()
        {
            if (path == null)
                Calculate();
            
            return path;
        }

        public Vector2Int GetNextStepFrom(Vector2Int unitPos)
        {
            var pathArray = GetPath().ToArray();
            for (int i = 0; i < pathArray.Length; i++)
            {
                if (pathArray[i] == unitPos && i < pathArray.Length - 1)
                    return pathArray[i + 1];
            }

            var closestPoint = pathArray.OrderBy(p => (p - unitPos).sqrMagnitude).FirstOrDefault();
            return closestPoint != default ? closestPoint : unitPos;
        }

        protected BaseUnitPath(IReadOnlyRuntimeModel runtimeModel, Vector2Int startPoint, Vector2Int endPoint)
        {
            this.runtimeModel = runtimeModel;
            this.startPoint = startPoint;
            this.endPoint = endPoint;
        }
    }
}