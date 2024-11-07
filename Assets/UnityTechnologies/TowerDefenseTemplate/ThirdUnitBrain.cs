using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitBrains.Player
{
    // Перечисление возможных действий для юнита
    enum ThirdUnitBrainAction
    {
        Any,
        Attack,
        Move
    };

    public class ThirdUnitBrain : DefaultPlayerUnitBrain
    {
        
        public override string TargetUnitName => "Ironclad Behemoth";

        private ThirdUnitBrainAction AllowedAction = ThirdUnitBrainAction.Any; // Текущее разрешенное действие
        private float lastActionTime = 0f; // Время последнего действия

        // Метод обновления состояния юнита
        public override void Update(float deltaTime, float time)
        {
            // Если действие не разрешено, проверяем, истекло ли время
            if (AllowedAction != ThirdUnitBrainAction.Any)
            {
                if (time - lastActionTime >= 1)
                {
                    AllowedAction = ThirdUnitBrainAction.Any; // Сбрасываем действие
                }
            }
        }

        // Получение следующего шага юнита
        public override Vector2Int GetNextStep()
        {
            Vector2Int nextStepForMove = base.GetNextStep(); 
            
            if (AllowedAction == ThirdUnitBrainAction.Any && !Vector2Int.Equals(nextStepForMove, unit.Pos))
            {
                AllowedAction = ThirdUnitBrainAction.Move; 
            }

            
            if (AllowedAction == ThirdUnitBrainAction.Move && !Vector2Int.Equals(nextStepForMove, unit.Pos))
            {
                lastActionTime = Time.time; 
            }

            // Возвращаем следующий шаг или текущую позицию, если движение не разрешено
            return AllowedAction == ThirdUnitBrainAction.Move ? nextStepForMove : unit.Pos;
        }

        // Выбор целей для атаки
        protected override List<Vector2Int> SelectTargets()
        {
            List<Vector2Int> targets = base.SelectTargets(); 
            
            if (AllowedAction == ThirdUnitBrainAction.Any && targets.Count > 0)
            {
                AllowedAction = ThirdUnitBrainAction.Attack;
            }

           
            if (AllowedAction == ThirdUnitBrainAction.Attack && targets.Count > 0)
            {
                lastActionTime = Time.time;
            }

            
            return AllowedAction == ThirdUnitBrainAction.Attack ? targets : new List<Vector2Int>();
        }
    }
}