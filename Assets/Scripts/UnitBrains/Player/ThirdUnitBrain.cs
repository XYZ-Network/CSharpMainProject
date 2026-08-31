using Model.Runtime.Projectiles;
using System.Collections;
using System.Collections.Generic;
using UnitBrains.Player;
using UnityEngine;

public class ThirdUnitBrain : DefaultPlayerUnitBrain
{
    public override string TargetUnitName => "Ironclad Behemoth";

    // Режимы работы юнита
    private enum Mode
    {
        Moving,      // Движение
        Attacking,   // Атака
        Switching    // Переключение (ничего не делает)
    }

    private Mode _currentMode = Mode.Moving;   // Текущий режим
    private float _switchTimer = 0f;           // Таймер переключения
    private const float SwitchDuration = 1f;   // Длительность переключения (1 секунда)

    // Вызывается каждый кадр для обновления состояния
    public override void Update(float deltaTime, float time)
    {
        // Вызов базового обновления (если нужно)
        base.Update(deltaTime, time);

        // Проверяем, есть ли цель в радиусе атаки
        bool hasTargetInRange = CheckTargetInAttackRange();

        // Определяем желаемый режим
        Mode desiredMode = hasTargetInRange ? Mode.Attacking : Mode.Moving;

        // Если желаемый режим отличается от текущего и мы не в процессе переключения
        if (desiredMode != _currentMode && _currentMode != Mode.Switching)
        {
            // Начинаем переключение
            _currentMode = Mode.Switching;
            _switchTimer = 0f;
        }

        // Если мы в процессе переключения
        if (_currentMode == Mode.Switching)
        {
            _switchTimer += deltaTime;
            if (_switchTimer >= SwitchDuration)
            {
                // Завершаем переключение на желаемый режим
                _currentMode = desiredMode;
                _switchTimer = 0f;
            }
        }
    }

    // Проверяет наличие цели в радиусе атаки
    private bool CheckTargetInAttackRange()
    {
        var allTargets = GetAllTargets();
        foreach (var target in allTargets)
        {
            if (IsTargetInRange(target))
                return true;
        }
        return false;
    }

    // Определяет следующий шаг движения
    public override Vector2Int GetNextStep()
    {
        // Если режим движения — двигаемся (используем базовую логику)
        // Если режим атаки или переключения — стоим на месте
        if (_currentMode == Mode.Moving)
        {
            return base.GetNextStep();
        }
        else
        {
            // Стоим на месте
            return unit.Pos;
        }
    }

    // Генерирует снаряды для атаки
    protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
    {
        // Стреляем только в режиме атаки
        if (_currentMode == Mode.Attacking)
        {
            // Используем базовую генерацию снарядов
            base.GenerateProjectiles(forTarget, intoList);
        }
        // В остальных режимах ничего не делаем
    }

    // Возвращает список целей для атаки
    protected override List<Vector2Int> SelectTargets()
    {
        // Возвращаем цели только если мы в режиме атаки,
        // иначе возвращаем пустой список (чтобы не тратить ресурсы)
        if (_currentMode == Mode.Attacking)
        {
            return base.SelectTargets();
        }
        else
        {
            return new List<Vector2Int>();
        }
    }
}
