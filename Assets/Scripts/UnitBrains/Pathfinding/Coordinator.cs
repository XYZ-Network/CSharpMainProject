using Model;
using Model.Runtime.ReadOnly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Utilities;

namespace Assets.Scripts.UnitBrains.Pathfinding
{
    public class Coordinator
    {
        //// Приватное статическое поле для хранения единственного экземпляра
        //private  Coordinator _instance;
        //получаем  сведения всей текущей сессии
        private IReadOnlyRuntimeModel _runtimeModel;
        
        //private IReadOnlyMap _roMap;
        // Получаем нашу базу (PlayerId)
        private Vector2Int OurBase => _runtimeModel.RoMap.Bases[RuntimeModel.PlayerId];

        // Получаем вражескую базу (BotPlayerId)
        private Vector2Int EnemyBase => _runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId];

        private TimeUtil _timeUtil;
        // Приватный конструктор - нельзя создать извне
        public Coordinator(IReadOnlyRuntimeModel runtimeModel) 
        {
            Console.WriteLine("Создан экземпляр SingltonCoordinator");
            _runtimeModel = runtimeModel;

            //_runtimeModel = ServiceLocator.Get<IReadOnlyRuntimeModel>();
            

            // Регистрируем метод обновления в игровом цикле
            ServiceLocator.Get<TimeUtil>().AddUpdateAction(Update);
        }

       
        // Получаем всех вражеских юнитов
        private IEnumerable<IReadOnlyUnit> GetEnemyUnits()
        {
            return _runtimeModel.RoBotUnits;

        }
        // Получаем врагов на нашей половине карты
        private List<IReadOnlyUnit> GetEnemiesOnOurHalf()
        {
            return GetEnemyUnits()
                .Where(unit => IsOnOurHalf(unit.Pos))
                .ToList();
        }
        // Находим ближайшего врага к нашей базе
        private IReadOnlyUnit FindEnemyClosestToBase(IEnumerable<IReadOnlyUnit> enemies)
        {
            return enemies
                .OrderBy(unit => Vector2Int.Distance(unit.Pos, OurBase))
                .FirstOrDefault();
        }
        // Находим врага с наименьшим здоровьем
        private IReadOnlyUnit FindEnemyWithLowestHealth(IEnumerable<IReadOnlyUnit> enemies)
        {
            return enemies
                .OrderBy(unit => unit.Health)
                .FirstOrDefault();
        }
        
        // Проверяем, находится ли позиция на нашей половине карты
        private bool IsOnOurHalf(Vector2Int enemyPosition)
        {
            // Простая проверка: если ближе к нашей базе, чем к вражеской
            float distToOurBase = Vector2Int.Distance(enemyPosition, OurBase);
            float distToEnemyBase = Vector2Int.Distance(enemyPosition, EnemyBase);
            return distToOurBase < distToEnemyBase;
        }
        // Обычный метод класса
        public IReadOnlyUnit GetRecomendationTarget()
        {
            var enemiesOnOurHalf = GetEnemiesOnOurHalf();
            var allEnemies = GetEnemyUnits().ToList();
            //var attackRangeSqr = unitPos.Config.AttackRange * unitPos.Config.AttackRange;
            if (enemiesOnOurHalf.Count > 0)
            {
              
                // Если есть враги на нашей половине - атакуем ближайшего к базе
                return FindEnemyClosestToBase(enemiesOnOurHalf);
            }
            else if (allEnemies.Count > 0)
            {
                // Если врагов на нашей половине нет - атакуем самого слабого
                return FindEnemyWithLowestHealth(allEnemies);
            }

            return null; // Если врагов нет вообще
        }
        public Vector2Int GetRecommendedPosition()
        {
            var enemiesOnOurHalf = GetEnemiesOnOurHalf();
            var allEnemies = GetEnemyUnits().ToList();

            if (enemiesOnOurHalf.Count > 0)
            {
                // Если есть враги на нашей половине - становимся перед базой
                return OurBase;
            }
            else
            {

                // Если врагов нет - возвращаем позицию базы противника
                return EnemyBase;
            }
        }

        public void Update(float deltaTime)
        {
            //// Подписываемся на событие _updateAction
            GetRecomendationTarget();
            GetRecommendedPosition();
        }
    }
}
