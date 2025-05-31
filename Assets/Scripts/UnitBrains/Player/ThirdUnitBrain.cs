using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using Codice.Client.Common.GameUI;
using JetBrains.Annotations;
using Model;
using Model.Runtime.Projectiles;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Utilities;

namespace UnitBrains.Player
{
    public class ThirdUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Ironclad Behemoth";
        private float _changeStateTime = 1f;  //кэп времени для смены состояния
        private float _currentStateTime = 0f;  // текущее время в смене состояния
        private bool _isChanging = false;
        private enum UnitState  // текущий статус юнита передвижение, атака
            {
            Move,
            Attack
            }
        private UnitState _currentState = UnitState.Move; // выставляем в базу перемещение


        public override void Update(float deltaTime, float time)
        {
            //Debug.Log("time = "+ time);
            if (_isChanging)  // если текущий статус = смена состояния
            {
                //Debug.Log("check IsChanging true");
                _currentStateTime += Time.deltaTime*10;
                //Debug.Log("_currentStateTime = " + _currentStateTime);
                //if (_currentStateTime + _changeStateTime > time)
                if (_currentStateTime > _changeStateTime)
                {
                    //Debug.Log("time exeed changeTime");
                    ChangeState();
                }
            }
            else
            {
                //Debug.Log("HasTargetsInRange() = " + HasTargetsInRange());
                if (HasTargetsInRange() && _currentState == UnitState.Move)
                {
                    _isChanging = true;
                    _currentStateTime = 0f;
                }
                if (!HasTargetsInRange() && _currentState == UnitState.Attack)
                {
                    _isChanging = true;
                    _currentStateTime = 0f;
                }
            }
        }

        private void ChangeState()
        {
            //Debug.Log("enter the ChangeState");
            
            
            if (_currentState == UnitState.Attack && _isChanging)
            {
                //Debug.Log("toMove");
                _currentState = UnitState.Move;
                _isChanging = false;
            }

            if (_currentState == UnitState.Move && _isChanging)
            {
                //Debug.Log("toAttack");
                _currentState = UnitState.Attack;
                _isChanging = false;
            }
            
            
        }

        public override Vector2Int GetNextStep()
        {
            if (_currentState == UnitState.Attack)
            {
                return unit.Pos;
            }
            return base.GetNextStep();
        }

        protected override List<Vector2Int> SelectTargets()
        {
            if (_currentState == UnitState.Move)
            {
                return new List<Vector2Int>();
            }
            return base.SelectTargets();
        }

    }
}