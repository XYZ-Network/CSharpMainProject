using UnityEngine;

namespace UnitBrains.Pathfinding
{
    public class Tile
    {
        public Vector2Int coord; //координаты клетки
        public float distToTarget; //Дистанция до таргета
        public Tile prevTile; //Клетка, из которой мы сюда попали

        public Tile(Vector2Int _coord, Vector2Int _targetCoord, Tile _prevTile)
        {
            coord = _coord;
            distToTarget = (_coord - _targetCoord).magnitude;
            prevTile = _prevTile;
        }
    }
}