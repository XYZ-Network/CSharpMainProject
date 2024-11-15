using UnityEngine;
using Utilities;

namespace GameLogic
{
    public class UnitCoordinator : MonoBehaviour
    {
        private static UnitCoordinator _instance;

        public static UnitCoordinator Instance
        {
            get
            {
                if (_instance == null)
                {
                    throw new System.Exception("UnitCoordinator instance is null. Make sure it is in the scene.");
                }
                return _instance;
            }
        }

        public Vector2Int RecommendedTarget { get; private set; }
        public Vector2Int RecommendedPoint { get; private set; }

        private IReadOnlyRuntimeModel _runtimeModel;
        private TimeUtil _timeUtil;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);

                // Создаем экземпляр TimeUtil
                _timeUtil = TimeUtil.Create();

                // Подписываемся на обновления
                _timeUtil.AddUpdateAction(OnUpdate);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void Initialize(IReadOnlyRuntimeModel runtimeModel)
        {
            _runtimeModel = runtimeModel;
            UpdateRecommendations();
        }

        private void OnUpdate(float deltaTime)
        {
            // Логика, которая будет выполняться каждый кадр
            // Можете использовать deltaTime для расчетов
        }

        public void UpdateRecommendations()
        {
            Vector2Int[] enemies = _runtimeModel.GetEnemiesPositions();
            int playerId = _runtimeModel.GetPlayerId();
            Vector2Int basePosition = _runtimeModel.GetBasePosition(playerId);

            if (enemies.Length > 0 && IsEnemyOnOurHalf(enemies, basePosition))
            {
                RecommendedTarget = GetClosestEnemyToBase(enemies, basePosition);
                RecommendedPoint = basePosition + Vector2Int.up; // Или используйте нужное смещение
            }
            else
            {
                RecommendedTarget = GetWeakestEnemy(enemies);
                RecommendedPoint = GetPointNearWeakestEnemy(enemies, basePosition);
            }
        }

        private bool IsEnemyOnOurHalf(Vector2Int[] enemies, Vector2Int basePosition)
        {
            // Ваша логика проверки
            return true; // Пример
        }

        private Vector2Int GetClosestEnemyToBase(Vector2Int[] enemies, Vector2Int basePosition)
        {
            Vector2Int closestEnemy = enemies[0];
            float closestDistance = Vector2Int.Distance(basePosition, closestEnemy);

            foreach (Vector2Int enemy in enemies)
            {
                float distance = Vector2Int.Distance(basePosition, enemy);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = enemy;
                }
            }

            return closestEnemy;
        }

        private Vector2Int GetWeakestEnemy(Vector2Int[] enemies)
        {
            Vector2Int weakestEnemy = enemies[0];
            float lowestHealth = GetEnemyHealth(weakestEnemy);

            foreach (Vector2Int enemy in enemies)
            {
                float health = GetEnemyHealth(enemy);
                if (health < lowestHealth)
                {
                    lowestHealth = health;
                    weakestEnemy = enemy;
                }
            }

            return weakestEnemy;
        }

        private Vector2Int GetPointNearWeakestEnemy(Vector2Int[] enemies, Vector2Int basePosition)
        {
            Vector2Int weakestEnemy = GetWeakestEnemy(enemies);
            return weakestEnemy + Vector2Int.right; // Или используйте нужное смещение на расстоянии выстрела
        }

        private float GetEnemyHealth(Vector2Int enemyPos)
        {
            return _runtimeModel.GetEnemyHealth(enemyPos);
        }

        private void OnDestroy()
        {
            // Отписываемся от обновлений
            if (_timeUtil != null)
            {
                _timeUtil.RemoveUpdateAction(OnUpdate);
            }
        }
    }
}