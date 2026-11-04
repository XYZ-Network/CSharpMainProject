using Model;
using Model.Runtime.Projectiles;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace UnitBrains.Player
{
    public class ThirdUnitBrain : DefaultPlayerUnitBrain
        {
        public override string TargetUnitName => "Ironclad Behemoth"; //имя юнита. Должно быть идентичным с именем в комноненте юнита.
        private const float SwitchDuration = 1f;
        private Mode _currentMode = Mode.Moving;
        private Mode _targetMode = Mode.Moving;
        private float _switchEndTime = 0f;
        private enum Mode 
        {
            Moving,
            Attacking,
            Switching
        }

        public override void Update(float deltaTime, float time)
        {
            base.Update(deltaTime, time); // На текущий момент в родителе метод Update ничего не полняет.
            if (_currentMode == Mode.Switching) // Сначало проверяем, что юнит в режиме переключения.
            {
                if (time >= _switchEndTime) // Если режим переключения, то проверяем, что текущее игровое время больше или равно времени на переключение (время на переключение считает текущее игровое + 1 секунда)
                {
                    _currentMode = _targetMode; // Если текущее игровое время больше времени перехода, значит переход произашёл и текущий режим меняем на целевой режим.
                }
                return; // Прерываем выполнение метода, чтобы не апдейтить _switchEndTime. Иначе переход не закончится.
            }
            Mode expectedMode = IsTargetInRange(GetAssignedTarget()) ? Mode.Attacking : Mode.Moving; // Определяем в какой режим должен переключиться юнит. Если есть цель, то режим атаки в ином случае режим перемещения

            if (_currentMode != expectedMode) // Проверяем отличается ли текущий режим юнита от ожидаемого режима. 
            {
                Debug.Log("Текущий режим: " + _currentMode + " | " + "Следующий режим: " + expectedMode);
                _targetMode = expectedMode; // целевой режим становится ожидаемым
                _currentMode = Mode.Switching; // текущий режим становится переключением, так как между режимами атака и движение мы должны ожидать переключения режима
                _switchEndTime = time + SwitchDuration; // вычисляем через сколько режим переключения закончится.
            }

        }

        public override Vector2Int GetNextStep()
        {
            if (_currentMode != Mode.Moving)//  режим не в движение
            {
                return unit.Pos; //стоим на месте
            }

            return GetStepTowardsAssignedTarget();// идём к цели

        }

        protected override List<Vector2Int> SelectTargets()
        {
            if (_currentMode != Mode.Attacking)
                return new List<Vector2Int>();

            return GetAssignedTargetIfInRange();
        }

    }

}
