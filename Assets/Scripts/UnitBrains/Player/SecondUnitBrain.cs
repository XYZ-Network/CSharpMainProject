using System.Collections.Generic;
using Model.Runtime.Projectiles;
using UnityEngine;
using System.Linq;
using Model;                    // чтобы видеть RuntimeModel.PlayerId / BotPlayerId
using UnitBrains.Pathfinding;
using System.Collections.Generic;

namespace UnitBrains.Player
{
    public class SecondUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Cobra Commando";
        private const float OverheatTemperature = 3f;
        private const float OverheatCooldown = 2f;
        private float _temperature = 0f;
        private float _cooldownTime = 0f;
        private bool _overheated;
        
        //private readonly System.Collections.Generic.List<Vector2Int> _pendingTargets = new System.Collections.Generic.List<Vector2Int>();
        private readonly List<Vector2Int> _pendingTargets = new List<Vector2Int>(); 
        private Vector2Int? _currentObjective;
        
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
            var all = GetAllTargets(); // враги + база противника
            var enemyBase = runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId];
            var enemiesOnly = all.Where(t => t != enemyBase);
            var myBase = runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.PlayerId : RuntimeModel.BotPlayerId];
            var target = (enemiesOnly.Any() ? enemiesOnly : new[] { enemyBase })
                .OrderBy(t => (t - myBase).sqrMagnitude)
                .First();
            _currentObjective = target;
            _pendingTargets.Clear();
            var result = new List<Vector2Int>();
            if (IsTargetInRange(target)) result.Add(target); else _pendingTargets.Add(target);
            return result;
            ///////////////////////////////////////
        }

        public override void Update(float deltaTime, float time)
        {
            if (_overheated)
            {              
                _cooldownTime += Time.deltaTime;
                float t = _cooldownTime / (OverheatCooldown/10);
                _temperature = Mathf.Lerp(OverheatTemperature, 0, t);
                if (t >= 1)
                {
                    _cooldownTime = 0;
                    _overheated = false;
                }
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
    }
}