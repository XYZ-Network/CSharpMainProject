using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.BuffsAndDebuffs;
using Model;
using Model.Runtime;
using Model.Runtime.Projectiles;
using Model.Runtime.ReadOnly;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;
using Utilities;

enum UnitBufferState
{
    Move,
    Shoot,
    Buff,
    PrepareBuff,
    MoveCooldown
}

namespace UnitBrains.Player
{
    public class UnitBuffer : DefaultPlayerUnitBrain
    {
        private bool _readyToBuff = false;
        private bool _readyToMove = true;

        private float _stateÑhangeTime = .2f;
        private float _stateÑhangeTimer = 0;

        private float _buffPause = 0;
        private float _buffPauseTimer = 0.5f;

        public override string TargetUnitName => "UnitBuffer";
        private UnitBufferState _state = UnitBufferState.Move;
        private BuffAndDebuffControllSystem _buffAndDebuffControllSystem;
        public UnitBuffer()
        {
            _buffAndDebuffControllSystem = ServiceLocator.Get<BuffAndDebuffControllSystem>();
        }

        protected override List<Vector2Int> SelectTargets()
        {
            if (_state == UnitBufferState.Shoot)
                return base.SelectTargets();
            return new List<Vector2Int>();
        }

        public override Vector2Int GetNextStep()
        {
            return _state == UnitBufferState.Move ? base.GetNextStep() : unit.Pos;
        }

        public override void Update(float deltaTime, float time)
        {
            var unitsInRadius = GetUnitsInRadius(unit.Config.AttackRange, true);
            switch (_state)
            {
                case UnitBufferState.Move:
                    if (_stateÑhangeTimer >= _stateÑhangeTime)
                    {
                        _readyToBuff = true;
                    }
                    _stateÑhangeTimer += deltaTime;
                    if (unitsInRadius.Any() && _readyToBuff)
                    {
                        _state = UnitBufferState.PrepareBuff;
                    }
                    break;
                case UnitBufferState.PrepareBuff:
                    if (_buffPause >= _buffPauseTimer)
                    {
                        _buffPause = 0;
                        _state = UnitBufferState.Buff;
                    }
                    _buffPause += deltaTime;
                    break;
                case UnitBufferState.Buff:
                    if (unitsInRadius.Any())
                    {
                        foreach (var unit in unitsInRadius)
                        {
                            TryApplyAnEffect(unit as Unit);
                        }
                        _state = UnitBufferState.MoveCooldown;
                    }
                    else
                    {
                        _state = UnitBufferState.Move;
                    }
                    break;
                case UnitBufferState.MoveCooldown:
                    if (_buffPause >= _buffPauseTimer)
                    {
                        _buffPause = 0;
                        _state = UnitBufferState.Move;
                        _readyToBuff = false;
                        _stateÑhangeTimer = 0;
                    }
                    _buffPause += deltaTime;
                    break;
            }
        }

        public void TryApplyAnEffect(Unit unit)
        {
            if (!_buffAndDebuffControllSystem.CheckUnitInEffectList(unit))
            {
                float randomNumber = UnityEngine.Random.Range(0, 10);

                _buffAndDebuffControllSystem.AddItem(unit, randomNumber >= 5 ? new MovementBuff<Unit>(unit) : new AttackBuff<Unit>(unit));
            }
        }
    }
}