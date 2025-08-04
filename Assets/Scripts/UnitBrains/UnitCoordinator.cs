using Model;
using Model.Runtime.ReadOnly;
using PlasticPipe.PlasticProtocol.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.PlayerLoop;
using UnityEngine.UIElements;
using Utilities;
using static UnityEngine.EventSystems.EventTrigger;

namespace Assets.Scripts.UnitBrains
{
    public class UnitCoordinator
    {
        public Vector2Int? RecomendedTarget { get; private set; }
        public Vector2Int? RecomendedPosition { get; private set; }
        private readonly IReadOnlyRuntimeModel _runtimeModel;
        private readonly TimeUtil _timeUtil;
        public UnitCoordinator()
        {
            _runtimeModel = ServiceLocator.Get<IReadOnlyRuntimeModel>();
            _timeUtil = ServiceLocator.Get<TimeUtil>();
            _timeUtil.AddFixedUpdateAction(GetRecomendation);
        }
        private void GetRecomendation(float dt)
        {
            List<IReadOnlyUnit> enemies = _runtimeModel.RoBotUnits.ToList();

            if (enemies.Count == 0)
            {
                RecomendedTarget = null;
                RecomendedPosition = null;
                return;
            }

            int ourHalf = _runtimeModel.RoMap.Width / 2;
            bool enemyOnOurHalf = enemies.Any(enemy => enemy.Pos.y < ourHalf);

            Vector2Int ourBase = _runtimeModel.RoMap.Bases[RuntimeModel.PlayerId];
            Vector2Int direction = _runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId] - ourBase;

            enemies.OrderBy(enemy => CalculateDistance(enemy.Pos, ourBase));
            Vector2Int closestEnemyPosition = enemies[0].Pos;

            if (enemyOnOurHalf)
            {
                RecomendedTarget = enemies[0].Pos;
                RecomendedPosition = ourBase + NormalizeVector(direction);
            }
            else
            {
                enemies.OrderBy(enemy => enemy.Health).ThenBy(enemy => CalculateDistance(enemy.Pos, ourBase));

                RecomendedTarget = enemies[0].Pos;
                RecomendedPosition = closestEnemyPosition - NormalizeVector(direction);
            }
        }
        private double CalculateDistance(Vector2Int unitPos, Vector2Int basePos)
        {
            return Math.Abs(Math.Sqrt(Math.Pow(unitPos.x - basePos.x, 2))) + Math.Abs(Math.Sqrt(Math.Pow(unitPos.y - basePos.y, 2)));
        }

        private Vector2Int NormalizeVector(Vector2Int vector)
        {
            Vector2Int absVector = new(Math.Abs(vector.x), Math.Abs(vector.y));
            int maxDist = Math.Max(absVector.x, absVector.y);
            int minDist = Math.Min(absVector.x, absVector.y);
            if (maxDist / minDist < 2f)
                return new Vector2Int(vector.x > 0 ? 1 : -1, vector.y > 0 ? 1 : -1);
            else
            {
                if (maxDist == absVector.x)
                    return new Vector2Int(vector.x > 0 ? 1 : -1, 0);
                else
                    return new Vector2Int(0, vector.y > 0 ? 1 : -1);
            }
        }
    }
}
