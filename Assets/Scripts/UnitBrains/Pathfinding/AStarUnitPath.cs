using System.Collections.Generic;
using System.Linq;
using Model;
using UnityEngine;

namespace UnitBrains.Pathfinding
{
    public class AStarUnitPath : BaseUnitPath
    {
        private Vector2Int[] _directions = new Vector2Int[] {new Vector2Int(1,0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1) };//направления движения

        public AStarUnitPath(IReadOnlyRuntimeModel runtimeModel, Vector2Int startPoint, Vector2Int endPoint) : base(runtimeModel, startPoint, endPoint)
        {
        }

        protected override void Calculate()
        {
            CalcPath(false);
            if (path == null)
                CalcPath(true);

            if (path == null)
            {
                path = new Vector2Int[] { startPoint, startPoint };
                return; //попробуем снова на следующей прокрутке, может быть ситуация поменяется
            }
        }

        private void CalcPath(bool ignoreUnits)
        { 
            List<Tile> ReachableTiles = new List<Tile> { new Tile(startPoint, endPoint, null) }; //Известные клетки. На старте нам известна только стартовая точка
            List<Tile> ReachedTiles = new List<Tile> {}; //изученые координаты
             
            Tile endTile = null;


            //Пока у нас есть клетки для исследования - бесконечно проверяем список доступных)
            while (ReachableTiles.Count > 0)
            {
                //найдем ближайшую не изученую клетку 
                Tile closedTile = getClosedTile(ReachableTiles);

                ReachableTiles.Remove(closedTile); //уберем из известных клеток closedTile

                //добавим в список изученых координат. Понадобитсья ниже что бы избежать повторного изучения тайлов
                ReachedTiles.Add(closedTile);

                //исследуем наш endTile и добавим соседние клеки в ReachableTiles
                //проверяем каждую клетку на существование и дсотупность, перед добавлением. (По 4 направлениям)
                foreach (Vector2Int dir in _directions) {
                    Vector2Int newCoord = closedTile.coord + dir;

                    if (newCoord == endPoint)  
                    {
                        endTile = new Tile(newCoord, newCoord, closedTile);
                        ReachableTiles.Clear(); 
                        break;
                    }
                    
                    bool exist = !
                        (
                       newCoord.x < 0 || newCoord.x >= runtimeModel.RoMap.Width ||
                       newCoord.y < 0 || newCoord.y >= runtimeModel.RoMap.Height
                       );
                    if (!exist)
                    {
                        continue; 
                    }

                    if (runtimeModel.RoMap[newCoord] || (runtimeModel.RoUnits.Any(u => u.Pos == newCoord) && !ignoreUnits))
                    {
                        continue;
                    }
                                        
                    //Если клетка не изучена и существует на карте - то добавляем в список доступных к изучению
                    
                    if (!ReachableTiles.Any(pos => pos.coord.x == newCoord.x && pos.coord.y == newCoord.y)
                        && !ReachedTiles.Any(pos => pos.coord.x == newCoord.x && pos.coord.y == newCoord.y)) 
                        
                    {
                        ReachableTiles.Add(new Tile(newCoord, endPoint, closedTile));
                    }

                }

            }



            //В endTile по .prevTile можем выстроить путь от конечной до начальной точки
            if (endTile != null)
            {
                var result = new List<Vector2Int> { }; //наш путь
                while (endTile.prevTile != null)
                {
                    result.Add(endTile.coord);
                    endTile = endTile.prevTile;
                }
                result.Add(startPoint);
                result.Reverse();
                path = result.ToArray();

            }

        }

        private Tile getClosedTile(List<Tile> reachableTiles)
        {
            Tile ClosedTile = reachableTiles[0];
            foreach (Tile tile in reachableTiles)
            {
                if (tile.distToTarget < ClosedTile.distToTarget)
                    ClosedTile = tile;
            }

            return ClosedTile;
        }
    }
}
