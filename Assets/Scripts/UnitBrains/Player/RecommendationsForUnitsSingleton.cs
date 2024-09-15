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
    public Vector2Int _playerBasePos { get; private set; }
    public Vector2Int _enemyBasePos { get; private set; }

    private event Action UpdateRuntimeModelEvent;

    private static RecommendationsForUnitsSingleton _instance;
    private RecommendationsForUnitsSingleton()
    {
        _runtimeModel = ServiceLocator.Get<IReadOnlyRuntimeModel>();
        _timeUtil = ServiceLocator.Get<TimeUtil>();
        UpdateRuntimeModelEvent += UpdateRuntimeModel;

        _timeUtil.AddFixedUpdateAction(UpdateRuntimeModelWrapperForEvent);

        _playerBasePos = _runtimeModel.RoMap.Bases[RuntimeModel.PlayerId];
        _enemyBasePos = _runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId];
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

    public static RecommendationsForUnitsSingleton GetInstance()
    {
        if (_instance == null)
        {
            _instance = new RecommendationsForUnitsSingleton();
        }

        return _instance;
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
        Vector2Int centerPointBetweenBases = _playerBasePos + (_playerBasePos - _enemyBasePos) / 2; // центр карты
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
