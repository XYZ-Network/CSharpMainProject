using System.Collections.Generic;
using Model.Runtime.Projectiles;
using UnityEngine;

namespace UnitBrains.Player
{
    public class ThirdUnitBrain : DefaultPlayerUnitBrain
    {
        //имя юнит
        public override string TargetUnitName => "Ironclad Behemoth";

        //Время переключения между режимами
        private const float SwitchDuration = 1f;

        //Перечисление состояний юнита
        private enum UnitState
        {
            Moving,             //Едет
            SwitchingToShoot,   //Переключается, чтобы стрелять
            Shooting,           //Стреляет
            SwitchingToMove     //Переключается, чтобы ехать
        }

        //Текущее состояние
        private UnitState _currentState = UnitState.Moving;

        //Таймер для времени переключения
        private float _switchTimer = 0f;

        public override void Update(float deltaTime, float time)
        {        
            base.Update(deltaTime, time);

            switch (_currentState)
            {
                case UnitState.Moving:
                    //Когда юнит едет, но увидел врага в зоне атаки, то начинает переключаться на стрельбу
                    if (HasTargetsInRange())
                    {
                        _currentState = UnitState.SwitchingToShoot;
                        _switchTimer = 0f;
                    }
                    break;

                case UnitState.SwitchingToShoot:
                    //Одна секунда переключения
                    _switchTimer += deltaTime;
                    if (_switchTimer >= SwitchDuration)
                    {
                        _currentState = UnitState.Shooting;
                    }
                    break;

                case UnitState.Shooting:
                    //Посрелял юнит, врагов в зоне видимости больше нет, то переключился на движение
                    if (!HasTargetsInRange())
                    {
                        _currentState = UnitState.SwitchingToMove;
                        _switchTimer = 0f;
                    }
                    break;

                case UnitState.SwitchingToMove:
                    //Однасекунда переключения
                    _switchTimer += deltaTime;
                    if (_switchTimer >= SwitchDuration)
                    {
                        _currentState = UnitState.Moving;
                    }
                    break;
            }
        }

        public override Vector2Int GetNextStep()
        {
            //Юнит может двигаться ТОЛЬКО в состоянии Moving
            //В остальных состояниях он стоит на месте
            if (_currentState == UnitState.Moving)
            {
                return base.GetNextStep();
            }
            else
            {
                return unit.Pos; //Стоим на месте
            }
        }

        protected override List<Vector2Int> SelectTargets()
        {
            //Юнит может стрелять ТОЛЬКО в состоянии Shooting
            //В остальных состояниях он возвращает пустой список
            if (_currentState == UnitState.Shooting)
            {
                return base.SelectTargets();
            }
            else
            {
                return new List<Vector2Int>(); //Пустой список, то бишь нет выстрелов
            }
        }
    }
}