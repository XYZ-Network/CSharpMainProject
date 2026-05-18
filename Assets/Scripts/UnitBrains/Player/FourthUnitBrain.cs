using System.Collections.Generic;
using Model.Runtime.Projectiles;
using UnityEngine;
using System.Linq;
using Model;                    // чтобы видеть RuntimeModel.PlayerId / BotPlayerId
using UnitBrains.Pathfinding;
using System.Collections.Generic;
using Model.Runtime.ReadOnly;
using Controller;
using View;
using Utilities;
using Effects;

namespace UnitBrains.Player
{
    public class FourthUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Buffer";
        private const float OverheatTemperature = 3f;
        private const float OverheatCooldown = 2f;
        private float _temperature = 0f;
        private float _cooldownTime = 0f;
        private bool _overheated;
        private static int s_unitCounter = 0;
        private int _unitNumber;
        private const int MAX_SMART_TARGETS = 3;
        private float _buffTimer = 0f;
        private const float BuffInterval = 0.05f;
        private const float BuffStopTime = 0.5f;
        private float _busyUntil = 0f;
        private float _castAt = 0f;
        private bool _isBusy = false;
        private IReadOnlyUnit _pendingBuffTarget;
                
        private readonly List<Vector2Int> _pendingTargets = new List<Vector2Int>(); 
        private Vector2Int? _currentObjective;
		private VFXView _vfxView;

        public FourthUnitBrain() { _unitNumber = s_unitCounter++; }
        
        private void SortByDistanceToOwnBase(List<Vector2Int> list)
        {
            var myBase = runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.PlayerId : RuntimeModel.BotPlayerId];
            list.Sort((a, b) => ((a - myBase).sqrMagnitude).CompareTo((b - myBase).sqrMagnitude));
        }

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;

            float currentTemperature = GetTemperature();
            if (currentTemperature >= overheatTemperature) { return; }

            for (float i = -1; i < currentTemperature; i++)
            {
                ///////////////////////////////////////
                // Homework 1.3 (1st block, 3rd module)
                ///////////////////////////////////////      
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
                ///////////////////////////////////////
            }
            IncreaseTemperature();
        }

        public override Vector2Int GetNextStep()
        {
            if (_isBusy) return unit.Pos;
            if (_currentObjective == null && _pendingTargets.Count > 0) _currentObjective = _pendingTargets[0];
            if (_currentObjective == null) return unit.Pos;
            if (IsTargetInRange(_currentObjective.Value)) return unit.Pos;
            var path = new DummyUnitPath(runtimeModel, unit.Pos, _currentObjective.Value);
            return path.GetNextStepFrom(unit.Pos);
        }

        protected override List<Vector2Int> SelectTargets()
        {
            ///////////////////////////////////////
            // Homework 1.4 (1st block, 4rd module)
            ///////////////////////////////////////
            _pendingTargets.Clear();
            var goals = new List<Vector2Int>();
            foreach (var t in GetAllTargets()) goals.Add(t);
            if (goals.Count == 0)
            {
                var enemyBase = runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId];
                goals.Add(enemyBase);
            }
            SortByDistanceToOwnBase(goals);
            int idx = _unitNumber % MAX_SMART_TARGETS;
            if (idx >= goals.Count) idx = 0;
            var chosen = goals[idx];
            _currentObjective = chosen;
            var result = new List<Vector2Int>();
            if (IsTargetInRange(chosen)) _pendingTargets.Add(chosen);
            return result;
            ///////////////////////////////////////
        }

        public override void Update(float deltaTime, float time)
        {
            _isBusy = time < _busyUntil;

            if (_isBusy)
            {
                if (_pendingBuffTarget != null && time >= _castAt)
                {
                    if (IsTargetInRange(_pendingBuffTarget.Pos))
                        BuffAlly(_pendingBuffTarget);
                    else
                        _busyUntil = time;
                    _pendingBuffTarget = null;
                }
                return;
            }

            _buffTimer += deltaTime;
            if (_buffTimer < BuffInterval) return;
            _buffTimer = 0f;

            var allies = GetUnitsInRadius(unit.Config.AttackRange, true);
            if (allies.Count == 0) return;

            var buffSystem = ServiceLocator.Get<BuffSystemController>();
            var target = allies.FirstOrDefault(a => !buffSystem.HasBuff(a));
            if (target != null)
            {
                _pendingBuffTarget = target;
                _castAt = time + BuffStopTime;
                _busyUntil = time + BuffStopTime * 2f;
                _isBusy = true;
            }
        }

        private int GetTemperature()
        {
            if(_overheated) return (int) OverheatTemperature;
            else return (int)_temperature;
        }

        private void IncreaseTemperature()
        {
            _temperature += 1f;
            if (_temperature >= OverheatTemperature) _overheated = true;
        }

		private void BuffAlly(IReadOnlyUnit unit)
		{
			_vfxView = ServiceLocator.Get<VFXView>();
			_vfxView.PlayVFX(unit.Pos, VFXView.VFXType.BuffApplied);
			var buffs = ServiceLocator.Get<BuffSystemController>();
            var buff = new IncreaseAttackSpeedBuff { modifier = 0.5f, duration = 5f };
            buffs.AddBuff(unit, buff);
		}
    }
}