using System.Collections;
using System.Collections.Generic;
using Model.Runtime;
using UnityEngine;

namespace UnitBrains.Player
{
    public enum ActionState // состояние действий юнита
    {
        Moving,
        Attacking,
        Switching
    }
    public class ThirdUnitBrain : DefaultPlayerUnitBrain // базовый класс для всех мозгов юнитов
    {
        public ActionState _state = ActionState.Moving; // Текущее состояние юнита (по умолчанию — движение)
        public ActionState _stateBeforeSwitch; //предыдущее состояние( состояние до начала переключения)
        public float _switchTimer = 0f; //таймер для отсчета переключения
        private const float SwitchDuration = 0.1f; //длительность переключения
        public override string TargetUnitName => "Ironclad Behemoth";

        public override Vector2Int GetNextStep() // отвечает за выбор ячейки для передвижения
        {
            if (_state != ActionState.Moving)
                return unit.Pos;

            return base.GetNextStep();
        }
        protected override List<Vector2Int> SelectTargets() //отвечает за выбор целей для атаки
        {
            if (_state != ActionState.Attacking)
                return new List<Vector2Int>();

            return base.SelectTargets();
        }
        public override void Update(float deltaTime, float time)
        {
            base.Update(deltaTime, time);

            switch (_state)
            {
                case ActionState.Moving:
                    if (base.HasTargetsInRange())
                    {
                        StartSwitching(ActionState.Attacking);
                    }
                    break;

                case ActionState.Attacking:
                    if (!base.HasTargetsInRange())
                    {
                        StartSwitching(ActionState.Moving);
                    }
                    break;

                case ActionState.Switching:
                    _switchTimer -= deltaTime;
                    if (_switchTimer <= 0)
                    {
                        _state = _nextStateAfterSwitch;
                    }
                    break;
            }
        }
        private ActionState _nextStateAfterSwitch;
        private void StartSwitching(ActionState nextState)
        {
            _stateBeforeSwitch = _state;
            _state = ActionState.Switching;
            _nextStateAfterSwitch = nextState;
            _switchTimer = SwitchDuration;
        }
    }
}
