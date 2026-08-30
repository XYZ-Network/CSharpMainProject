using System.Collections.Generic;
using Model;
using UnityEngine;
using Utilities;
using System.Collections;
using System;
using Codice.Client.BaseCommands;

namespace UnitBrains.Pathfinding
{
    public class SmartUnitPath : BaseUnitPath
    {
        private Vector2Int[] dx = {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.right,
            Vector2Int.left,
        };


        private const int MaxLenght = 100;

        public SmartUnitPath(IReadOnlyRuntimeModel runtimeModel, Vector2Int startPoint, Vector2Int endPoint) : base(runtimeModel, startPoint, endPoint)
        {
        }

        protected override void Calculate()
        {
            if (FindPath().Count < 0)
            {
                path = null;
            }
            else
            {
                path = FindPath().ToArray();
            }
        }

        public List<Vector2Int> FindPath()
        {
            Tile startTile = new Tile(startPoint);
            Tile targetTile = new Tile(endPoint);

            List<Tile> openList = new List<Tile> { startTile };
            List<Tile> closedList = new List<Tile>();
            int counter = 0;

            Tile farestTile = startTile;
            float farestDistance = float.MaxValue;

            startTile.CalculateEstimate(targetTile.Pos);
            startTile.CalculateValue();

            while (openList.Count > 0 && counter < MaxLenght)
            {
                Tile currentTile = openList[0];
                int index = 0;

                for (int i = 1; i < openList.Count; i++)
                {
                    if (openList[i].Value < currentTile.Value)
                    {
                        currentTile = openList[i];
                        index = i;
                    }

                }

                if (currentTile.Pos == targetTile.Pos)
                {
                    return GetPath(currentTile);
                }

                openList.RemoveAt(index);
                closedList.Add(currentTile);

                float currentDistance = CalcDistanceToTarget(currentTile.Pos, endPoint);
                if (currentDistance < farestDistance)
                {
                    farestDistance = currentDistance;
                    farestTile = currentTile;
                }

                if (endPoint.Equals(currentTile.Pos))
                {
                    return GetPath(currentTile);
                }

                foreach (var direction in dx)
                {
                    var nextPos = currentTile.Pos + direction;

                    if (!runtimeModel.IsTileWalkable(nextPos) && !nextPos.Equals(endPoint))
                    {
                        continue;
                    }

                    Tile neighbor = new Tile(nextPos);
                    if (closedList.Contains(neighbor))
                    {
                        continue;
                    }

                    bool isInOpenList = false;
                    for (int i = 0; i < openList.Count; i++)
                    {
                        if (openList[i].Equals(neighbor))
                        {
                            var newCost = currentTile.Cost + neighbor.Cost;
                            if (newCost < openList[i].Cost)
                            {
                                openList[i].Parent = neighbor;
                                openList[i].Cost = newCost;
                                openList[i].CalculateValue();
                            }
                            isInOpenList = true;
                            break;
                        }
                    }
                    if (!isInOpenList)
                    {
                        neighbor.Parent = currentTile;
                        neighbor.CalculateEstimate(targetTile.Pos);
                        neighbor.CalculateValue();
                        openList.Add(neighbor);
                    }
                }
                counter++;
            }
            return GetPath(farestTile);
        }

        private float CalcDistanceToTarget(Vector2Int x, Vector2Int y)
        {
            var diff = x - y;
            return Mathf.Sqrt(diff.x * diff.x + diff.y * diff.y);
        }

        protected List<Vector2Int> GetPath(Tile tile)
        {
            List<Vector2Int> result = new List<Vector2Int>();
            while (tile != null)
            {
                result.Add(tile.Pos);
                tile = tile.Parent;
            }
            result.Reverse();
            return result;
        }
    }
}
