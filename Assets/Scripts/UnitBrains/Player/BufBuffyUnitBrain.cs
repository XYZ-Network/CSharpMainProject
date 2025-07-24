using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Model.Runtime;
using Model.Runtime.ReadOnly;
using Systems.Buffs;
using UnityEngine;
using Utilities;
using View;
using Random = System.Random;

namespace UnitBrains.Player
{
    enum BuffType
    {
        SpeedUp,
        SlowDown,
        AttackSpeedUp,
        AttackSlowDown,
    }

    enum BufBuffyActionState
    {
        Idle,
        Cooldown
    }

    enum BufBuffyMoveState
    {
        Moving,
        Deploying,
        Buffing,
        Undeploying,
    }

    class BufBuffyMoveStateMachine : StateMachine<BufBuffyMoveState>
    {
        public BufBuffyMoveStateMachine() : base(BufBuffyMoveState.Moving)
        {
            Register(BufBuffyMoveState.Moving, BufBuffyMoveState.Deploying);
            Register(BufBuffyMoveState.Deploying, BufBuffyMoveState.Buffing, BufBuffyMoveState.Moving);
            Register(BufBuffyMoveState.Buffing, BufBuffyMoveState.Undeploying, BufBuffyMoveState.Deploying);
            Register(BufBuffyMoveState.Undeploying, BufBuffyMoveState.Moving);
        }
    }

    class BufBuffyActionStateMachine : StateMachine<BufBuffyActionState>
    {
        public BufBuffyActionStateMachine() : base(BufBuffyActionState.Idle)
        {
            Register(BufBuffyActionState.Idle, BufBuffyActionState.Cooldown);
            Register(BufBuffyActionState.Cooldown, BufBuffyActionState.Idle);
        }
    }

    public class BufBuffyUnitBrain : DefaultPlayerUnitBrain
    {
        private static List<Vector2Int> _emptyTargets = new();

        public override string TargetUnitName => "BufBuffy";
        private const float SwitchStateDurationSec = 0.5f;
        private const float ActionCooldownSec = 5f;

        private readonly BufBuffyMoveStateMachine _moveState = new();
        private readonly BufBuffyActionStateMachine _actionState = new();

        private float _prevGameTime;

        private readonly VFXView _vfxView = ServiceLocator.Get<VFXView>();
        private readonly BuffSystem _buffSystem = ServiceLocator.Get<BuffSystem>();
        private readonly Random _random = new();

        private BuffType[] _buffs = { BuffType.SpeedUp, BuffType.AttackSpeedUp };
        private BuffType[] _debuffs = { BuffType.SlowDown, BuffType.AttackSlowDown };

        public override void Update(float deltaTime, float time)
        {
            float gameDeltaTime = time - _prevGameTime;
            _prevGameTime = time;

            // Delta time is always 0.033 (30fps), so to use game time (not real time), we should use
            // `time` - seems it scales with `EnterPoint._timeScale`. Exactly what's needed.
            base.Update(deltaTime, time);

            _moveState.Update(gameDeltaTime);
            _actionState.Update(gameDeltaTime);

            var reachableAlly = GetClosestUnbuffedUnitInRange();
            bool hasAllyInRange = reachableAlly != null;

            switch (_moveState.State)
            {
                case BufBuffyMoveState.Moving:
                    if (hasAllyInRange && _actionState.State == BufBuffyActionState.Idle)
                    {
                        _moveState.Go(BufBuffyMoveState.Deploying);
                        _moveState.GoDelayed(BufBuffyMoveState.Buffing, SwitchStateDurationSec);
                    }

                    break;

                case BufBuffyMoveState.Deploying:
                    if (!hasAllyInRange)
                    {
                        // Damn. All units in range were destroyed or moved further. Drop everything and move!
                        _moveState.Go(BufBuffyMoveState.Moving);
                    }

                    break;

                case BufBuffyMoveState.Buffing:
                    if (hasAllyInRange && _actionState.State == BufBuffyActionState.Idle)
                    {
                        // TODO: move to better place - buff shouldn't be applied inside brain.
                        //   As far as i understood, brain should give just decision, and action should be done
                        //   by Unit, but this Buffer unit is too specific, to need to dive deeper on this matter.
                        Buff buff = GetBuffForUnit(reachableAlly);

                        _buffSystem.ApplyBuff(reachableAlly, buff);
                        _vfxView.PlayVFX(reachableAlly.Pos, VFXView.VFXType.BuffApplied);
                        
                        Debug.Log($"Applied buff, {buff}");

                        _actionState.Go(BufBuffyActionState.Cooldown);
                        _actionState.GoDelayed(BufBuffyActionState.Idle, ActionCooldownSec);

                        // After buffing unit check if we can buff anyone else without moving 
                        var anotherReachableAlly = GetClosestUnbuffedUnitInRange();

                        if (anotherReachableAlly != null)
                        {
                            _moveState.Go(BufBuffyMoveState.Undeploying);
                            _moveState.GoDelayed(BufBuffyMoveState.Moving, SwitchStateDurationSec);
                        }
                        else
                        {
                            // Go again to deploying stage and wait there deployed.
                            _moveState.Go(BufBuffyMoveState.Deploying);
                        }
                    }

                    break;

                case BufBuffyMoveState.Undeploying:
                    // Do nothing here. Even if we have some units nearby - we need to complete undeployment.
                    // Also, we get here from Buffing state, so it means that we're on cooldown and most probably
                    // our unit will move to some other unit.
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private Buff GetBuffForUnit(IReadOnlyUnit target)
        {
            var arr = target.Config.IsPlayerUnit ? _buffs : _debuffs;
            var buffType = arr[_random.Next(arr.Length)];

            // TODO: actual values should be defined in some config file, so game designer can play
            //   with values without recompilation of project.
            switch (buffType)
            {
                case BuffType.SpeedUp: return Buff.SpeedUp(2f, 1.3f);
                case BuffType.SlowDown: return Buff.SlowDown(2f, 0.7f);
                case BuffType.AttackSpeedUp: return Buff.AttackSpeedUp(3f, 1.5f);
                case BuffType.AttackSlowDown: return Buff.AttackSlowDown(3f, 0.5f);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override Vector2Int GetNextStep()
        {
            return _moveState.State != BufBuffyMoveState.Moving
                ? unit.Pos
                : base.GetNextStep();
        }

        protected override List<Vector2Int> SelectTargets()
        {
            // This unit won't shoot, so disable this ability.
            return _emptyTargets;
        }

        [CanBeNull]
        private IReadOnlyUnit GetClosestUnbuffedUnitInRange()
        {
            var buffRangeSqr = unit.Config.AttackRange * unit.Config.AttackRange;

            foreach (var possibleTarget in GetAllUnits())
            {
                var diff = possibleTarget.Pos - unit.Pos;

                if (diff.sqrMagnitude < buffRangeSqr && !_buffSystem.HasAnyBuff(possibleTarget))
                {
                    return possibleTarget;                    
                }
            }

            return null;
        }

        /// <summary>
        /// Returns all units with player units in the beginning of the list, as we have priority to buff own units.
        /// </summary>
        /// <returns></returns>
        protected IEnumerable<IReadOnlyUnit> GetAllUnits()
        {
            return runtimeModel.RoUnits
                .Where(u => u.Config.Name != TargetUnitName)
                .OrderBy(u => !u.Config.IsPlayerUnit);
        }
    }
}