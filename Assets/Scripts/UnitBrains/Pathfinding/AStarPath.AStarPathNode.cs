using System;
using UnityEngine;

namespace Assets.Scripts.UnitBrains.Pathfinding
{
    public partial class AStarPath
    {
        public class AStarPathNode
        {
            public const int Cost = 10;
            private int _estimate;

            public AStarPathNode(Vector2Int vector2)
            {
                X = vector2.x;
                Y = vector2.y;
            }

            public AStarPathNode(int x, int y)
            {
                X = x;
                Y = y;
            }

            public int X { get; }
            public int Y { get; }

            public AStarPathNode Parent { get; set; }
            public int Value { get; private set; }

            public int Estimate
            {
                get => _estimate;
                private set
                {
                    if (_estimate != value)
                    {
                        _estimate = value;
                        CalculateValue();
                    }
                }
            }

            public void CalculateEstimate(int targetX, int targetY)
            {
                Estimate = Math.Abs(X - targetX) + Math.Abs(Y - targetY);
            }

            public override bool Equals(object? obj)
            {
                if (obj is not AStarPathNode node)
                {
                    return false;
                }

                return X == node.X && Y == node.Y;
            }

            private void CalculateValue()
            {
                Value = Cost * Estimate;
            }

            public Vector2Int ToVector2()
            {
                return new Vector2Int(X, Y);
            }
        }
    }
}
