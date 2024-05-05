using System.Collections.Generic;
using System.Linq;
using Model;
using Model.Config;
using Model.Runtime.Projectiles;
using Model.Runtime.ReadOnly;
using UnitBrains;
using UnitBrains.Pathfinding;
using UnityEngine;
using Utilities;

public class UnitsCoordinator : BaseUnitBrain
{

    private IReadOnlyRuntimeModel _runtimeModel = ServiceLocator.Get<IReadOnlyRuntimeModel>();
    private IReadOnlyUnit _runtimePos;
    private Vector2Int _playerBase;
    private Vector2Int _enemyBase;
    private Vector2Int _unitPos;
    private int _health; 

    public UnitsCoordinator()
    {
        
    }


    public Vector2Int FindTargetEnemy()
    {
        IEnumerable<Vector2Int> unitPositions = GetAllTargets(); //создаем список всех доступных врагов
        Vector2Int closestEnemyPosition = new Vector2Int(); //создаем позицию ближайшего врага
        float closestDistance = float.MaxValue; // Инициализируем максимально возможным значением


        foreach (var unitPos in unitPositions)
        {
            Vector2 baseToUnitVector = _unitPos - _playerBase; // Вычисляем вектор от вашей базы к юниту и также преобразуем его в Vector2
            Vector2 baseToBaseVector = _enemyBase - _playerBase;  // Получаем вектор от вашей базы к базе врага и преобразуем его в Vector2 для дальнейших расчетов

            Vector2 normalizedBaseToBaseVector = baseToBaseVector.normalized; // Нормализуем вектор от базы к базе

            float dotProduct = Vector2.Dot(normalizedBaseToBaseVector, baseToUnitVector);

            float distanceToBase = baseToUnitVector.magnitude; //Рассчитываем расстояние от базы до юнита

            if (dotProduct <= 0)
            { // Юнит находится на нашей стороне
                if (distanceToBase < closestDistance)
                {
                    closestDistance = distanceToBase;
                    closestEnemyPosition = unitPos;
                }
            }
        }

        if (closestDistance != float.MaxValue)
        {
            Debug.Log($"Ближайший враг для атаки находится на позиции: {closestEnemyPosition}");
            return closestEnemyPosition;
        }
        else
        {
            Debug.Log("На нашей стороне враги не найдены.");
            return Vector2Int.zero; 
        }

    }


}
