using Assets.Scripts.UnitBrains.Pathfinding;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitBrains.Pathfinding;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace Assets.Scripts.UnitBrains
{
    public class SmartMoveUnitBrain : BaseUnitPath //реализация А* для вертолетов

    {
        private int[] dx = { -1, 0, 1, 0 };
        private int[] dy = { 0, 1, 0, -1 };

        private readonly Vector2Int _playerBase;
        private readonly Vector2Int _enemyBase;
        private readonly Vector2Int _currentUnitPos;

        public SmartMoveUnitBrain(
            IReadOnlyRuntimeModel runtimeModel,
            Vector2Int startPoint,
            Vector2Int endPoint,
            Vector2Int currentUnitPos)
            : base(runtimeModel, startPoint, endPoint)
        {
            _currentUnitPos = currentUnitPos;
            _playerBase = runtimeModel.RoMap.Bases[RuntimeModel.PlayerId];
            _enemyBase = runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId];
        }
        private int CalculateHeuristic(Vector2Int from, Vector2Int to)
        {
            return Math.Abs(from.x - to.x) + Math.Abs(from.y - to.y); //формула
        }

        public override void Calculate()
        {
            var openList = new List<PathNode>();
            var closedList = new HashSet<Vector2Int>();

            var startNode = new PathNode(startPoint)
            {
                StartCost = 0,
                HashCost = CalculateHeuristic(startPoint, endPoint)
            };

            openList.Add(startNode);

            int iteration = 0;
            while (openList.Count > 0) 
            {
                iteration++;
                var currentNode = openList[0];
                for (int i = 1; i < openList.Count; i++)
                {
                    if (openList[i].FullCost < currentNode.FullCost ||
                        (openList[i].FullCost == currentNode.FullCost && openList[i].HashCost < currentNode.HashCost))
                    {
                        currentNode = openList[i];
                    }
                }

                openList.Remove(currentNode);
                closedList.Add(currentNode.Position);
                if (currentNode.Position == endPoint)
                {
                    path = ReconstructPath(currentNode).ToArray();
                    return; 
                }

                for (int i = 0; i < dx.Length; i++)
                {
                    Vector2Int neighborPos = new Vector2Int(
                        currentNode.Position.x + dx[i],
                        currentNode.Position.y + dy[i]
                    );
                    if (!IsValid(neighborPos) || closedList.Contains(neighborPos))
                        continue;

                    int newStartCost = currentNode.StartCost + 1;
                    var neighborNode = openList.Find(n => n.Position == neighborPos);

                    if (neighborNode == null)
                    {
                        neighborNode = new PathNode(neighborPos)
                        {
                            Parent = currentNode,
                            StartCost = newStartCost,
                            HashCost = CalculateHeuristic(neighborPos, endPoint)
                        };
                        openList.Add(neighborNode);
                       
                    }
                    else if (newStartCost < neighborNode.StartCost)
                    {
                        neighborNode.StartCost = newStartCost;
                        neighborNode.Parent = currentNode;
                      
                    }
                }
            }

           
            path = new Vector2Int[0];
        }
        private bool IsValid(Vector2Int pos)
        {

            if (pos.x < 0 || pos.x >= runtimeModel.RoMap.Width || pos.y < 0 || pos.y >= runtimeModel.RoMap.Height)
                return false;

            if (pos == endPoint)
                return true;

            if (runtimeModel.RoMap[pos])
                return false;

            bool isOnOwnBase = (_currentUnitPos == _playerBase) || (_currentUnitPos == _enemyBase);
            bool isNearOwnBase =
    Vector2Int.Distance(_currentUnitPos, _playerBase) <= 3 ||
    Vector2Int.Distance(_currentUnitPos, _enemyBase) <= 3;
            if (isOnOwnBase || isNearOwnBase)
                return true;

            
            foreach (var u in runtimeModel.RoUnits)
            {
                if (u.Pos == _currentUnitPos) continue; // не блокировать себя
                if (u.Pos == pos) return false;        // остальные стена чтоб эффективно обходить
            }

            return true;
        }
        private List<Vector2Int> ReconstructPath(PathNode endNode)
        {
            var path = new List<Vector2Int>();
            var currentNode = endNode;

            while (currentNode != null)
            {
                path.Add(currentNode.Position);
                currentNode = currentNode.Parent;
            }

            path.Reverse();

            return path;
        }
        public static BaseUnitPath GetPath(IReadOnlyRuntimeModel runtimeModel, Vector2Int startPoint, Vector2Int endPoint, Vector2Int currentUnitPos)
        {
            var pathBrain = new SmartMoveUnitBrain(runtimeModel, startPoint, endPoint, currentUnitPos);
            pathBrain.Calculate();
            return pathBrain;
        }
    }
}
