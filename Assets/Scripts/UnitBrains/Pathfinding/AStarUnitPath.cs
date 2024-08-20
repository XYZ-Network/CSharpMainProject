using Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using UnityEngine;
using Utilities;

namespace UnitBrains.Pathfinding
{
    public class AStarUnitPath : BaseUnitPath
    {
        public int Cost = 10;
        private int[] dx = { -1, 0, 1, 0 };
        private int[] dy = { 0, 1, 0, -1 };
        public AStarUnitPath(IReadOnlyRuntimeModel runtimeModel, Vector2Int startPoint, Vector2Int endPoint) : base(runtimeModel, startPoint, endPoint)
        {
        }
        protected override void Calculate()
        {
            if (FindPath() is not null)
            {
                path = FindPath().ToArray();
            }
            else
            {
                path = null;
            }

            if (path == null)
                path = new Vector2Int[] { StartPoint };

        }

        private List<Vector2Int> FindPath()
        {
            Vector2Int currentPoint = startPoint;
            List<Vector2Int> result = new List<Vector2Int> { startPoint };
            Vector2Int current = new Vector2Int();
            List<Vector2Int> steps = new List<Vector2Int>();

            while (currentPoint != endPoint)
            {
                
                for (int i = 0; i < 4; i++)
                {
                    Vector2Int newStep = new Vector2Int(currentPoint.x + dx[i], currentPoint.y + dy[i]);

                    if (runtimeModel.IsTileWalkable(newStep))
                    {
                        if (result.Contains(newStep))
                        {
                            continue;
                        }
                        if (!steps.Contains(newStep))
                        {
                            steps.Add(newStep);
                        }

                    }
                }

                foreach (var step in steps)
                {
                    if (CalculateValue(step, endPoint) < CalculateValue(currentPoint, endPoint))
                    {
                        current = step;
                    }
                }


                
                var hasLoop = result.Contains(current);
                result.Add(current);
                if (hasLoop)
                    break;
                currentPoint = current;
            }

            return result;



        }
        public int CalculateEstimate(Vector2Int current, Vector2Int target)
        {
            return Math.Abs(current.x - target.x) + Math.Abs(current.y - target.y);
        }

        public int CalculateValue(Vector2Int current, Vector2Int target)
        {
            return Cost + CalculateEstimate(current, target);
        }
    }
}
