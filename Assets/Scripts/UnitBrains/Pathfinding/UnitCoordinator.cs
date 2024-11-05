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
    private IReadOnlyRuntimeModel _runtimeModel;

    private void Awake()
    {
        // Получаем зависимости
        _runtimeModel = ServiceLocator.Get<IReadOnlyRuntimeModel>();
        if (_runtimeModel == null)
        {
            Debug.LogError("Failed to get IReadOnlyRuntimeModel from ServiceLocator.");
        }
    }

    public (Vector2Int recommendedTarget, Vector2Int recommendedPosition) GetRecommendations(int attackRadius, int mapHalf)
    {
        var enemyUnits = GetEnemyUnits();
        Vector2Int playerBasePos = GetPlayerBasePosition();

        Vector2Int recommendedTarget;
        Vector2Int recommendedPosition;

        if (enemyUnits.Any(unit => unit.Pos.x < mapHalf))
        {
            // Есть враги на нашей половине карты
            recommendedTarget = GetClosestEnemy(enemyUnits, playerBasePos);
            recommendedPosition = playerBasePos + new Vector2Int(-1, 0); // Точка перед базой
        }
        else
        {
            // Враги на другой стороне
            recommendedTarget = GetWeakestEnemy(enemyUnits);
            recommendedPosition = recommendedTarget + new Vector2Int(0, 1); // Точка на расстоянии выстрела от ближайшего врага
        }

        return (recommendedTarget, recommendedPosition);
    }

    private List<Unit> GetEnemyUnits()
    {
        return _runtimeModel.RoUnits
            .OfType<Unit>() // Приводим к типу Unit
            .ToList(); // Сохраняем полные объекты, а не только позиции
    }

    private Vector2Int GetPlayerBasePosition()
    {
        // Используем фиксированную позицию для базы
        Vector2Int playerBasePosition = new Vector2Int(0, 0); // Замените на вашу реальную позицию
        return playerBasePosition;
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