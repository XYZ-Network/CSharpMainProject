using GluonGui.Dialog;
using Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.Plastic.Antlr3.Runtime.Tree;
using UnityEngine;
namespace UnitBrains.Player
{
    public class ThirdUnitBrain : DefaultPlayerUnitBrain
    {
        private float transitionTime = 1f;         //Общее время перехода, которое юнит ждёт между состояниями
        private float transitionTimeLeft;          //Отсчитывает сколько времени осталось
        private enum UnitState // Пробую что-то типа FSM
        {
            Move,
            Attack,
            Transition          // Переименовал со Switching потому что путалось "состояние переключения" с логикой "переключения состояний"
                                // Transition - состояние паузы между атакой и движением
        }
        public override string TargetUnitName => "Ironclad Behemoth";      //Имя юнита
        private UnitState unitState = UnitState.Move;                      // текущее состояние
      
        public override void Update(float deltaTime, float time)
        {            
            switch (unitState)                                              //Логика перехода из одного состояния в другое
            {
                case UnitState.Move: 
                    MoveState();
                    break;
                case UnitState.Attack:
                    AttackState();
                    break;
                case UnitState.Transition:                                  //Представляю переход как отдельное, третье состояние, а не просто как каую-то логику где-то между атакой и движением
                    TransitionState();
                    break;
            }

        }
        public override Vector2Int GetNextStep()
        {
            if (unitState == UnitState.Move) return base.GetNextStep(); //Идём как обычно если состояние "идти"
            return unit.Pos;                                            //Иначе стоим на месте и ждём (атака или переход - не важно, всё равно стоим)
        }

        protected override List<Vector2Int> SelectTargets()
        {

            List<Vector2Int> result = new();
            if (unitState == UnitState.Attack) return base.SelectTargets(); // Выбираем цели для атаки как обычно, если сейчас атакуем
            return result;                                                  // Иначе возвращаем пустой список 
            
        }
        public void MoveState()
        {
            if (HasTargetsInRange())                                        //Если в зоне досягаемости есть цели - метод проверки из базового класса
            {
                unitState = UnitState.Transition;                           //Переключаем состояние и запускаем таймер простоя
                StartTransitionTimer();
            } 

        }
        public void TransitionState()
        {                                                           //timeMultiplier - Магическое число, но таймер считает медленнее чем должен
            float timeMultiplier = 10f;                             //В логике перегрева из SecondUnitBrain (cтрока 128) это тоже учтено
            transitionTimeLeft -= (Time.deltaTime*timeMultiplier);  //Сделаю так же
            if ( transitionTimeLeft <= 0f)
            {
                if (HasTargetsInRange()) 
                {                                     
                    unitState = UnitState.Attack;       //Если есть цели в зоне поражения - переключаем состояние на атаку
                }
                else unitState = UnitState.Move;        //Если целей для атаки нет - возвращаемся к движению
                transitionTimeLeft = 0f;                //Для красоты
            }
        }
        public void AttackState()
        {       
            if (!HasTargetsInRange())    //Если целей в зоне поражения не осталось 
            {
                unitState = UnitState.Transition;           //Переходим в режим переключения и запускаем таймер
                StartTransitionTimer();
            }  
        }

        private void StartTransitionTimer()                 //Отдельный метод чтобы не хардкодить в нескольких местах время в одну секунду ожидания
        {
            transitionTimeLeft = transitionTime;
        }               
    }
}