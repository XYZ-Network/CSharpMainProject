using Model;
using Model.Runtime.ReadOnly;
using System;
using System.Collections.Generic;
using System.Linq;

using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnitBrains.Pathfinding;
using UnityEngine;

namespace Assets.Scripts.UnitBrains
{
    public class MyUnitBrain : BaseUnitPath
    {

        private readonly int[] cord = { 1, 0, -1, 0 };
        PathAStar PathAStarPath;
        PathAStar EmdPath;
        bool IsPlayerUnitBrain;

        public MyUnitBrain(IReadOnlyRuntimeModel runtimeModel, Vector2Int startPoint, Vector2Int endPoint, bool _isPlayerUnitBrain) : base(runtimeModel, startPoint, endPoint) 
        { 
        IsPlayerUnitBrain = _isPlayerUnitBrain;
        } 
        protected override void Calculate()
        {
            var pathresult = GetAStarPath();
            if (pathresult.Count >= 2)
            {
                path = pathresult.ToArray();
            }
            else
            {
                path = new Vector2Int[] { startPoint, startPoint };
            }

        }
           
        
        public List<Vector2Int> GetAStarPath()
        {
            List<Vector2Int> pathresult;
            CalculAStarPath();
            if (EmdPath != null)
            {
                pathresult = (EmdPath.GetPathAStar());
            }
            else
                pathresult = new List<Vector2Int>();
            return pathresult;
        }

        private void CalculAStarPath()
        {
            var current_startPoint= startPoint;
            PathAStar PathAStarPath = new PathAStar(runtimeModel, current_startPoint, endPoint, null, 0);
            PathAStarPath.FullValue = 0;
            List<PathAStar> closed = new List<PathAStar>();
            List<PathAStar> opened = new List<PathAStar>();
            closed.Add(PathAStarPath);
            while (true)
            {
                PathAStar neighbor;
                closed.Add(PathAStarPath);
                for (int i = 0; i != 4; i++)
                {
                    neighbor = new PathAStar(runtimeModel, new Vector2Int((current_startPoint.x + cord[i]), (current_startPoint.y + cord[3 - i])), endPoint, PathAStarPath, 1);
                    neighbor.Calculete(endPoint, IsPlayerUnitBrain);
                    foreach (var item in closed)
                    {
                        if (item.MyPoint == neighbor.MyPoint)
                            neighbor.FullValue = -1;
                    }
                    foreach (var item in opened)
                    {
                        if (item.MyPoint == neighbor.MyPoint)
                            neighbor.FullValue = -1;
                    }
                    if (neighbor.FullValue != -1)
                    {
                        opened.Add(neighbor);
                    }
                }
                opened.Sort((x, y) => x.FullValue.CompareTo(y.FullValue));

                if (opened.Count() > 0)
                {
                    PathAStarPath = opened[0];
                    opened.RemoveAt(0);
                    current_startPoint = PathAStarPath.MyPoint;
                }
                else
                {
                    EmdPath = null;
                    break;
                }
                if (PathAStarPath.MyPoint == endPoint)
                {
                    EmdPath = PathAStarPath;
                    break;
                }
                
            }
        }
        protected class PathAStar
        {
            PathAStar Parent;
            float Value;
            public float FullValue = -1;
            public float ToTargetValue;
            public Vector2Int MyPoint;
            IReadOnlyRuntimeModel runtimeModel;
            public PathAStar(IReadOnlyRuntimeModel _runtimeModel, Vector2Int current_startPoint, Vector2Int endPoint, PathAStar parent, int value)
            {
                runtimeModel = _runtimeModel;
                Value = value;
                ToTargetValue = Math.Abs(current_startPoint.x - endPoint.x)+ Math.Abs(current_startPoint.y - endPoint.y);
                Parent = parent;
                MyPoint = new Vector2Int(current_startPoint.x, current_startPoint.y);
            }
            public float Calculete(Vector2Int endpos, bool IsPlayerUnitBrain)
            {

                if (runtimeModel.IsTileWalkable(MyPoint) || MyPoint == endpos)
                {
                    FullValue = Value + ToTargetValue;
                }
                else
                {
                    foreach (var item in IsPlayerUnitBrain ? runtimeModel.RoBotUnits : runtimeModel.RoPlayerUnits)
                    {
                        if (item.Pos == MyPoint)
                        {
                            FullValue = Value + ToTargetValue;
                            return FullValue;
                        }
                    }
                }
                return FullValue;
            }
            public List<Vector2Int> GetPathAStar()
            {
                var list = new List<Vector2Int>();
                while (Parent != null)
                {
                    list.Add(Parent.MyPoint);
                    Parent = Parent.Parent;
                }
                list.Reverse();
                return list;
            }
        
        }
    }


}
