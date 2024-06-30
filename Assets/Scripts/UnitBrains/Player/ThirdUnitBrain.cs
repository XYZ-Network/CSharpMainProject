using System.Collections.Generic;
using System.Linq;
using Model.Runtime.Projectiles;
using UnityEngine;
using Utilities;

namespace UnitBrains.Player
{

    public enum UnitState {
        Move,
        Attack,
        // Switch
    }

    public class ThirdUnitBrain : DefaultPlayerUnitBrain {
        public override string TargetUnitName => "Ironclad Behemoth";
        // public static int UnitCounter = 0;
        // private int UnitID = UnitCounter++;
        private bool _isSwitchingStates = false;
        private float StateSwitchCooldownTimeSeconds = .1f;
        private float _stateSwitchCooldownTime = 0f;
        private UnitState _nextUnitState = UnitState.Move;
        private UnitState _unitState = UnitState.Move;

        public void QueueStateSwitch(UnitState state) {
            if (state == _unitState || _isSwitchingStates) {
                return;
            }
            _isSwitchingStates = true;
            _nextUnitState = state;
        }

        private void TryToggleState() {
            if (_nextUnitState == _unitState || _isSwitchingStates) {
                return;
            }
            // Debug.Log($"Unit#{UnitID} toggling to State \"{_nextUnitState}\"");
            _unitState = _nextUnitState;
            _isSwitchingStates = false;
        }

        public override void Update(float deltaTime, float time)
        {
            base.Update(deltaTime, time);
            if (_isSwitchingStates) {
                _stateSwitchCooldownTime += Time.deltaTime;
                if (_stateSwitchCooldownTime >= StateSwitchCooldownTimeSeconds) {
                    _stateSwitchCooldownTime = 0;
                    _isSwitchingStates = false;
                }
            }
            TryToggleState();
        }

        protected override List<Vector2Int> SelectTargets()
        {
            var targets = base.SelectTargets();
            if (_unitState != UnitState.Attack) {
                if (targets.Any()) {
                    QueueStateSwitch(UnitState.Attack);
                }   
                return new();
            }
            if (!targets.Any()) {
                QueueStateSwitch(UnitState.Move);
            }
            return targets;
        }

        public override Vector2Int GetNextStep() {
            var nextStep = base.GetNextStep();
            if (_unitState != UnitState.Move) {
                return unit.Pos;
            }
            // if (nextStep == unit.Pos) {
            //     ToggleState(UnitState.Switch);
            // }
            return nextStep;
        }
    }
}