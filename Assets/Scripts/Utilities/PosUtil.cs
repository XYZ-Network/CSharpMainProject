using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Model;
using Model.Runtime.Projectiles;
using Unity.VisualScripting;
using UnityEngine;
using UnitBrains.Player;
using Model.Runtime;
using Utilities;
using System.Net;
using Assets.Scripts.UnitBrains.Pathfinding;
using Model.Runtime.ReadOnly;
using UnityEngine.UIElements;


namespace Assets.Scripts.Utilities
{
    public class PosUtil : MonoBehaviour
    {

        public PosUtil() 
        { 
        }
        private bool _onPlayerSide;
        private IReadOnlyRuntimeModel _runtimeModel = ServiceLocator.Get<IReadOnlyRuntimeModel>();
        List<Vector2Int> Bases;

        public Vector2Int UnitUtil(List<Vector2Int> units, List<Vector2Int> Bases, Vector2Int _bestTarget, Vector2Int _currentTarget)
        {
            _currentTarget = units[0];

            if(OnPlayerSide(_currentTarget, Bases))
            {
                _bestTarget = _currentTarget;
            }

            return _bestTarget;
        }

        public Point RecomendedPointUtil(List<Point> points, Point _currentPoint, Point _recomendedPoint)
        {
            _currentPoint = points[0];

            if(_onPlayerSide)
            {
                _recomendedPoint = _currentPoint;
            }
            
            return _recomendedPoint;
        }

        public bool IsBigger(Vector2Int a, Vector2Int b)
        {
            if (a - b != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool OnPlayerSide(Vector2Int target, IReadOnlyList<Vector2Int> Bases)
        {
            if (IsBigger(target - Bases[0], target - Bases[0]))
            {
                _onPlayerSide = true;
                return true;
            }
            else
            {
                _onPlayerSide  = false;
                return false;
            }
        }
        public void FixedUpdete(List<Vector2Int> units, Vector2Int _bestTarget, Vector2Int _currentTarget, List<Point> points, Point _currentPoint, Point _recomendedPoint)
        {
            Bases.Add(_runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId]);
            Bases.Add(_runtimeModel.RoMap.Bases[RuntimeModel.PlayerId]);

            
            UnitUtil(Bases, units, _bestTarget, _currentTarget);
            RecomendedPointUtil(points, _currentPoint, _recomendedPoint);
        }
    }

    
}
