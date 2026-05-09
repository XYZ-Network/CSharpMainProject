using Model;
using Model.Runtime.Projectiles;
using System.Collections.Generic;
using System.Linq;
using UnitBrains.Pathfinding;
using UnityEngine;
using UnityEngine.UIElements;
using Utilities;
using static UnityEngine.GraphicsBuffer;

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
        Vector2Int mostDangerousTarget;
        
        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            ///////////////////////////////////////
            // Homework 1.3 (1st block, 3rd module)
            ///////////////////////////////////////  
            
            if (GetTemperature() >= overheatTemperature)
                    return;

            IncreaseTemperature();
            
            for (int i = 0; i < _temperature; i++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }
            
            ///////////////////////////////////////
        }

        public override Vector2Int GetNextStep()
        {
            if (IsTargetInRange(mostDangerousTarget))
                return unit.Pos;

            var target = new DummyUnitPath(runtimeModel, unit.Pos, mostDangerousTarget);
            return target.GetNextStepFrom(unit.Pos);
            // return unit.Pos.CalcNextStepTowards(mostDangerousTarget);
        }

        protected override List<Vector2Int> SelectTargets()
        {
            ///////////////////////////////////////
            // Homework 1.4 (1st block, 4rd module)
            ///////////////////////////////////////

            var allTargets = GetAllTargets();

            if (!allTargets.Any())
                return new List<Vector2Int> { runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId] };

            float closestEnemyDistance = float.MaxValue;
            Vector2Int closestEnemy = Vector2Int.zero;

            foreach (var target in allTargets)
            {
                float checkEnemyDistance = DistanceToOwnBase(target);
                if (checkEnemyDistance < closestEnemyDistance)
                {
                    closestEnemyDistance = checkEnemyDistance;
                    closestEnemy = target;
                }
            }

            mostDangerousTarget = closestEnemy;

            if (IsTargetInRange(closestEnemy)) 
                return new List<Vector2Int> { closestEnemy };
            else
                return new List<Vector2Int>();

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