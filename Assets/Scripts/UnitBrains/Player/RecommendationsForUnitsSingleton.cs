using Model;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

public class RecommendationsForUnitsSingleton
{
    public IReadOnlyRuntimeModel _runtimeModel { get; private set; }
    private TimeUtil _timeUtil;
    private event Action UpdateRuntimeModelEvent;

    public RecommendationsForUnitsSingleton(RuntimeModel runtimeModel, TimeUtil timeUtil)
    {
        _runtimeModel = runtimeModel;
        _timeUtil = timeUtil;
        
        UpdateRuntimeModelEvent += UpdateRuntimeModel;

        _timeUtil.AddFixedUpdateAction(UpdateRuntimeModelWrapperForEvent);

    }
    ~RecommendationsForUnitsSingleton()
    {
        Debug.Log("Singleton delete");
    }

    public void Dispose()
    {
        _timeUtil.RemoveFixedUpdateAction(UpdateRuntimeModelWrapperForEvent);
        UpdateRuntimeModelEvent -= UpdateRuntimeModel;
    }

    public void UpdateRuntimeModelWrapperForEvent(float time) // обвертка
    {
        UpdateRuntimeModel();
    }

    public void UpdateRuntimeModel() // метод который будет обновлять runtimeModel из вне
    {
        _runtimeModel = ServiceLocator.Get<IReadOnlyRuntimeModel>();

    }

    public Vector2Int RecommendationTarget()
    {
        Vector2Int playerBasePos = _runtimeModel.RoMap.Bases[RuntimeModel.PlayerId];
        Vector2Int enemyBasePos = _runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId];

        Vector2Int centerPointBetweenBases = playerBasePos + (playerBasePos - enemyBasePos) / 2; // центр карты
        
        float minEnemyHealth = float.MaxValue; // минимальное здоровье врага
        Vector2Int targetPos = Vector2Int.zero; // целевая позиция

        foreach (var enemy in _runtimeModel.RoBotUnits)
        {
            float distance = Vector2Int.Distance(enemy.Pos, _runtimeModel.RoMap.Bases[RuntimeModel.PlayerId]); // дистанция между нашей базы и врагом

            
            if (distance <= centerPointBetweenBases.magnitude)
            {
                Debug.Log($"Enemy with pos = {enemy.Pos} on my side");
                return enemy.Pos; // Возвращаем позицию врага на стороне игрока
            }

            // Ищем врага с наименьшим здоровьем
            if (minEnemyHealth > enemy.Health)
            {
                minEnemyHealth = enemy.Health;
                targetPos = enemy.Pos; // Сохраняем позицию врага с наименьшим здоровьем
               
            }
        }

        // Если врагов на стороне игрока нет, атакуем врага с наименьшим здоровьем или базу, если никого не осталось

        return targetPos == Vector2Int.zero? _runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId] : targetPos;

    }
}
