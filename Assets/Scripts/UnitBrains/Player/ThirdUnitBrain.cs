using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Codice.Client.Common;
using Model;
using UnitBrains.Pathfinding;
using UnitBrains.Player;
using UnityEditor;
using UnityEngine;
using Time = UnityEngine.Time;

public class ThirdUnitBrain : DefaultPlayerUnitBrain
{    
    public enum UnitMode
    {
        Moving,
        Attacking,
        Switching,
    }

    public override string TargetUnitName => "Ironclad Behemoth";
    public override bool IsPlayerUnitBrain => true;

    private UnitMode _currentMode = UnitMode.Moving;
    private UnitMode _targetMode = UnitMode.Moving;

    private bool _isSwitching = false;
    private float _switchStartTime;

    private float TransitionDuration = 1.0f;

    
    private int _currentAmountOfTargets = 0;


    // Update is called once per frame
    public override void Update(float deltaTime, float time)
    {
        _currentAmountOfTargets = base.SelectTargets().Count;
        if (_currentAmountOfTargets > 0)
            _targetMode = UnitMode.Attacking;
        else _targetMode = UnitMode.Moving;

        if (_currentMode != _targetMode)
            SwitchModeRequest(_targetMode, Time.time);

        if (_isSwitching)
        {
            Debug.Log($"Switching time");
           
            if (Time.time - _switchStartTime >= TransitionDuration)
            {
                _currentMode = _targetMode;
                _isSwitching = false;
                Debug.Log($"Переход завершён: {_currentMode}");
            }
            else
                return;
        }

        switch (_currentMode)
        {
            case UnitMode.Switching:
                Debug.Log($"Current Mode: {_currentMode}");
                break;
            case UnitMode.Moving:
                GetNextStep();
                Debug.Log($"Current Mode: {_currentMode}");
                break;

            case UnitMode.Attacking:
                SelectTargets();
                Debug.Log($"Current Mode: {_currentMode}");
                break;
        }
    }
    protected override List<Vector2Int> SelectTargets()
    {

        if (_currentMode == UnitMode.Attacking)
        {
            return base.SelectTargets();
        }
        else
        {
            return new List<Vector2Int>();
        }

    }

    public override Vector2Int GetNextStep()
    {

        if (_currentMode == UnitMode.Moving)
        {
            return base.GetNextStep();
        }
        else
        {
            return unit.Pos;
        }

    }

    public void SwitchModeRequest(UnitMode desiredMode, float currentTime)
    {
        if (_isSwitching || _currentMode == desiredMode)
            return;

        _isSwitching = true;
        _currentMode = UnitMode.Switching;
        _switchStartTime = currentTime;
    }


}
