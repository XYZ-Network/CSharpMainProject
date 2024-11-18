using System.Collections.Generic;
using System.Linq;
using Model.Runtime; // Убедитесь, что это пространство имен правильно указано
using Model.Runtime.ReadOnly; // Для IReadOnlyRuntimeModel
using UnityEngine;
using Utilities;
using Model;
using Model.Config;
using View;

public class UnitCoordinator : MonoBehaviour
{
    public static UnitCoordinator Instance { get; private set; }

    private IReadOnlyRuntimeModel _runtimeModel;
    private TimeUtil _timeUtil;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Получаем зависимости
        _runtimeModel = ServiceLocator.Get<IReadOnlyRuntimeModel>();
        if (_runtimeModel == null)
        {
            Debug.LogError("Failed to get IReadOnlyRuntimeModel from ServiceLocator.");
        }
        _timeUtil = TimeUtil.Create(); // Создаем или получаем существующий экземпляр TimeUtil
    }

    public Vector2Int RecommendedTarget { get; private set; }
    public Vector2Int RecommendedPosition { get; private set; }

    public void UpdateRecommendations(int attackRadius, int mapHalf)
    {
        var enemyUnits = GetEnemyUnits();
        Vector2Int playerBasePos = GetPlayerBasePosition();

        if (enemyUnits.Any(unit => unit.Pos.x < mapHalf))
        {
            // Есть враги на нашей половине карты
            RecommendedTarget = GetClosestEnemy(enemyUnits, playerBasePos);
            RecommendedPosition = playerBasePos + new Vector2Int(-1, 0); // Точка перед базой
        }
        else
        {
            // Враги на другой стороне
            RecommendedTarget = GetWeakestEnemy(enemyUnits);
            RecommendedPosition = RecommendedTarget + new Vector2Int(0, 1); // Точка на расстоянии выстрела от ближайшего врага
        }
    }

    private List<Unit> GetEnemyUnits()
    {
        return _runtimeModel.RoUnits
            .OfType<Unit>() // Приводим к типу Unit
            .ToList(); // Сохраняем полные объекты, а не только позиции
    }

    private Vector2Int GetPlayerBasePosition()
    {
        var playerBase = _runtimeModel.RoBases.ElementAtOrDefault(RuntimeModel.PlayerId) as MainBase;
        if (playerBase == null)
        {
            Debug.LogError($"Player base not found for PlayerId: {RuntimeModel.PlayerId}");
            return Vector2Int.zero; // Возврат значения по умолчанию
        }
        return playerBase.Pos; // Получаем позицию базы игрока
    }

    private Vector2Int GetClosestEnemy(List<Unit> enemies, Vector2Int playerBase)
    {
        if (!enemies.Any())
        {
            Debug.LogWarning("No enemies found.");
            return Vector2Int.zero; // Возврат значения по умолчанию
        }

        return enemies
            .OrderBy(unit => Vector2Int.Distance(unit.Pos, playerBase))
            .FirstOrDefault()?.Pos ?? Vector2Int.zero;
    }

    private Vector2Int GetWeakestEnemy(List<Unit> enemies)
    {
        if (!enemies.Any())
        {
            Debug.LogWarning("No enemies found.");
            return Vector2Int.zero; // Возврат значения по умолчанию
        }

        return enemies
            .Where(unit => !unit.IsDead) // Убедимся, что юнит не мертв
            .OrderBy(unit => unit.Health) // Сортируем по здоровью
            .FirstOrDefault()?.Pos ?? Vector2Int.zero;
    }
}