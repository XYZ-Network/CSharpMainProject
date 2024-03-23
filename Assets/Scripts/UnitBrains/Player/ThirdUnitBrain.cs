using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Model;
using Model.Runtime.Projectiles;
using Unity.VisualScripting;
using UnityEngine;
using UnitBrains.Player;
using UnityEngine.XR;

namespace UnitBrains.Player
{
    public class ThirdUnitBrain : DefaultPlayerUnitBrain
    {
        public string TargetUnitBrain => "Ironclad Behemoth";

        private bool _hasTargets = false;
        bool IsUnitMoving = false;
        private float _timerStop = 0f;
        private float _stateChangeTime = 1f;

        public override Vector2Int GetNextStep()
        {
            if (IsUnitMoving)
            {
                return base.GetNextStep();

            }
            else
            {
                return unit.Pos;
            }
        }

        

        protected override List<Vector2Int> SelectTargets()
        {
            var result = base.SelectTargets();
            _hasTargets = result.Count > 0;

            if (IsUnitMoving) result.Clear();

            return result;
        }

        public void CurrentState()
        {
            if(_hasTargets == false)
            {
                _timerStop += Time.deltaTime;
                if(_timerStop > _stateChangeTime)
                {
                    IsUnitMoving = true;
                    _timerStop = 0f;
                }
            }
            else
            {
                _timerStop += Time.deltaTime;
                if (_timerStop < _stateChangeTime)
                {
                    IsUnitMoving = false;
                    _timerStop = 0f;
                }
            }
        }

        public override void Update(float deltaTime, float time)
        {
            base.Update(deltaTime, time);
            CurrentState();    
        }


    }

}