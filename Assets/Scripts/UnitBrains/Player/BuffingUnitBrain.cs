using Model;
using Model.Runtime;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace UnitBrains.Player
{
    internal class BuffingUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Buffing Bot";

        bool _freezing = false;
        float _freezingTime = 1f;
        float _freezingTimer = 0f;

        bool _canBuffing = true;
        float CDForBuffing = 0.5f;
        float CDForBuffingTimer = 0;
        public override Vector2Int GetNextStep()
        {
            if (!_freezing)
            {
                return base.GetNextStep();
            }
            return unit.Pos;
        }

        protected override List<Vector2Int> SelectTargets()
        {
            return new List<Vector2Int>();
        }
        public void AddEffect()
        {
            var effectController = ServiceLocator.Get<EffectController>();

            _freezingTimer = _freezingTime;
            CDForBuffingTimer = CDForBuffing;

            foreach (var target in ListUnitsInRange()) 
            {
                if (target.Config.Name == TargetUnitName) continue;
                effectController.AddEffect((Unit)target, EffectController.TypesOfEffects.DelayForNextMoveTimeEffect, 0.5f, 1);
            }
        }
        public override void Update(float deltaTime, float time)
        {

            if (HasMyUnitsInRange() && _canBuffing)
            {
                AddEffect();
            } 
            else if (!_freezing)
            {
                CDForBuffingTimer -= 0.25f;
            }
            if (!AnyMyUnitsInMap())
            {
                unit.TakeDamage(unit.Health);
            }
            _freezingTimer -= 0.25f;

            _freezing = _freezingTimer > 0;
            _canBuffing = CDForBuffingTimer <= 0;
            base.Update(deltaTime, time);
        }

        private bool HasMyUnitsInRange()
        {
            var attackRangeSqr = unit.Config.AttackRange * unit.Config.AttackRange;

            foreach (var possibleTarget in runtimeModel.RoPlayerUnits)
            {
                {
                    var diff = possibleTarget.Pos - unit.Pos;
                    if (diff.sqrMagnitude < attackRangeSqr)
                        return true;
                }
            }
                return false;
        }
        private List<Model.Runtime.ReadOnly.IReadOnlyUnit> ListUnitsInRange()
        {
            var attackRangeSqr = unit.Config.AttackRange * unit.Config.AttackRange;
            var Result = new List<Model.Runtime.ReadOnly.IReadOnlyUnit>();
            foreach (var possibleTarget in runtimeModel.RoPlayerUnits)
            {
                {
                    var diff = possibleTarget.Pos - unit.Pos;
                    if (diff.sqrMagnitude < attackRangeSqr)
                        Result.Add(possibleTarget);
                }
            }
            return Result;
        }
        private bool AnyMyUnitsInMap()
        {

            
            foreach (var possibleTarget in runtimeModel.RoPlayerUnits)
            {
                {
                    if (possibleTarget.Config.Name != TargetUnitName)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}

