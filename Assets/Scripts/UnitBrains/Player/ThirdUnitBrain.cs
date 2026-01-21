using System.Collections;
using System.Collections.Generic;
using Model;
using UnitBrains.Player;
using UnityEngine;
using Utilities;

public class ThirdUnitBrain : DefaultPlayerUnitBrain

{
    /// <summary>
    /// Имя целевой единицы. Всегда возвращает "Ironclad Behemoth".
    /// </summary>
    public string TargetUnitName => "Ironclad Behemoth";

    // Перечисление возможных состояний юнита
    private enum State
    {
        Moving,
        Attacking,
        Transitioning  // Состояние перехода между режимами
    }

    private State _currentState = State.Moving;  // Начальное состояние — движение
    private float _transitionTimer = 0f;       // Таймер перехода
    private const float TransitionDuration = 1f; // Длительность перехода (1 секунда)

    public virtual void Update()
    {

        // Обновляем таймер перехода, если находимся в состоянии перехода
        if (_currentState == State.Transitioning)
        {
            _transitionTimer += Time.deltaTime;

            // По истечении времени перехода переключаемся в целевой режим
            if (_transitionTimer >= TransitionDuration)
            {
                _currentState = _targetState;
                _transitionTimer = 0f;
                Debug.Log($"ThirdUnitBrain: Завершён переход в режим {_currentState}");
            }
        }

    }

    /// <summary>
    /// Запускает движение юнита (если возможно).
    /// </summary>
    public void Move()
    {
        if (_currentState == State.Moving)
        {
            Debug.Log("ThirdUnitBrain: Юнит движется по уникальной логике!");
            // Здесь размещается логика движения
        }
        else if (_currentState != State.Transitioning)
        {
            StartTransition(State.Moving);
        }
    }

    /// <summary>
    /// Запускает атаку юнита (если возможно).
    /// </summary>
    public void Attack()
    {
        if (_currentState == State.Attacking)
        {
            Debug.Log("ThirdUnitBrain: Юнит атакует с особым алгоритмом!");
            // Здесь размещается логика атаки
        }
        else if (_currentState != State.Transitioning)
        {
            StartTransition(State.Attacking);
        }
    }

    /// <summary>
    /// Начинает переход в указанное состояние.
    /// </summary>
    /// <param name="targetState">Целевое состояние после перехода.</param>
    private void StartTransition(State targetState)
    {
        _targetState = targetState;
        _currentState = State.Transitioning;
        _transitionTimer = 0f;
        Debug.Log($"ThirdUnitBrain: Начался переход в режим {targetState} (длительность: {TransitionDuration}с)");
    }

    private State _targetState;  // Целевое состояние после завершения перехода

    private void SpecialAbility()
    {
        Debug.Log("ThirdUnitBrain: Активирована особая способность!");
    }
    protected override List<Vector2Int> SelectTargets
    {
        get
        {

            var result = GetReachableTargets();

            if (result.Count > 1)
            {
                //находим ближайшую к своей базе
                Vector2Int closestToBase = result[0];
                float minDistanceToBase = DistanceToOwnBase(result[0]);

                for (int i = 1; i < result.Count; i++)
                {
                    float distanceToBase = DistanceToOwnBase(result[i]);
                    if (distanceToBase < minDistanceToBase)
                    {
                        minDistanceToBase = distanceToBase;
                        closestToBase = result[i];
                    }
                }

                //оставляем только ближайшую
                result = new List<Vector2Int> { closestToBase };
            }
            else if (result.Count == 0)
            {
                //если нет достижимых целей добывляем базу противника
                var enemyBase = runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId];
                result.Add(enemyBase);
            }
            return result;
        }
    }
    public override Vector2Int GetNextStep()
    {
        List<Vector2Int> targets = SelectTargets;

        if (targets.Count == 0)
        {
            return base.GetNextStep();
        }

        Vector2Int target = targets[0];
        bool isInAttackRange = true;
        Vector2Int position = Vector2Int.zero;
        Vector2Int nextposition = Vector2Int.right;
        position = position.CalcNextStepTowards(target);


        if (isInAttackRange)
        {
            return base.GetNextStep(); ;
        }
        else
        {
            return position.CalcNextStepTowards(target);
        }
    }
    
}



