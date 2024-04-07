using System.Collections.Generic;
using Model.Runtime.ReadOnly;
using UnityEngine;

namespace Model.Runtime
{
    public class Map : IReadOnlyMap
    {
        public bool this[int x, int y] => _map[x, y];

        public bool this[Vector2Int pos] =>
            pos.x < 0 || pos.x >= Width ||
            pos.y < 0 || pos.y >= Height ||
            _map[pos.x, pos.y];
        public int Width => _map.GetLength(0);
        public int Height => _map.GetLength(1);
        public IReadOnlyList<Vector2Int> Bases => _bases;
        
        private readonly bool[,] _map;
        private readonly List<Vector2Int> _bases = new();

        public Map(bool[,] map, int playersCount)
        {
            _map = map;
            
            PlaceBases(playersCount);
        }

        public Vector2Int FindFreeCellNear(Vector2Int pos, HashSet<Vector2Int> exclude = null)
        {
            var maxDistance = Mathf.Max(Width, Height);
            for (int shift = 0; shift < maxDistance; shift++)
            {
                for (int i = -shift; i <= shift; i++)
                {
                    var tp0 = pos + new Vector2Int(i, shift);
                    if (!this[tp0] && (exclude == null || !exclude.Contains(tp0)))
                        return tp0;
                    
                    var tp1 = pos + new Vector2Int(i, -shift);
                    if (!this[tp1] && (exclude == null || !exclude.Contains(tp1)))
                        return tp1;
                    
                    var tp2 = pos + new Vector2Int(shift, i);
                    if (!this[tp2] && (exclude == null || !exclude.Contains(tp2)))
                        return tp2;
                    
                    var tp3 = pos + new Vector2Int(-shift, i);
                    if (!this[tp3] && (exclude == null || !exclude.Contains(tp3)))
                        return tp3;
                }
            }
            
            throw new System.Exception("No free cell found");
        }

        private void PlaceBases(int basesCount)
        {
            for (int i = 0; i < basesCount; i++)
            {
                var refPoint = i % 2 == 0
                    ? new Vector2Int(Width / 2, 0)
                    : new Vector2Int(Width / 2, Height);
                var basePos = FindFreeCellNear(refPoint);
                _bases.Add(basePos);
                _map[basePos.x, basePos.y] = true;
            }
        }
    }
}