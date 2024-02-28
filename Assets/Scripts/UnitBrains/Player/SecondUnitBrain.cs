using System.Collections.Generic;
using Model;
using Model.Runtime.Projectiles;
using UnityEngine;
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
        List<Vector2Int> targets = new();

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            ///////////////////////////////////////
            // Homework 1.3 (1st block, 3rd module)
            ///////////////////////////////////////           
            var projectile = CreateProjectile(forTarget);
            AddProjectileToList(projectile, intoList);
            ///////////////////////////////////////
        }

        public override Vector2Int GetNextStep()
        {
            var position = targets.Count > 0 ? targets[0] : unit.Pos;

            if (IsTargetInRange(position))
            {
                return unit.Pos;
            }
            else
            {
                return unit.Pos.CalcNextStepTowards(position);
            }
        }

        protected override List<Vector2Int> SelectTargets()
        {
            List<Vector2Int> result = new();

            var maxPos = float.MaxValue;
            var needTarget = Vector2Int.zero;
            var enemyBase = Vector2Int.zero;

            foreach (var target in GetAllTargets())
            {
                var distance = DistanceToOwnBase(target);

                if (distance < maxPos)
                {
                    maxPos = distance;
                    needTarget = target;
                    targets.Add(needTarget);

                    if (IsTargetInRange(needTarget))
                    {
                        result.Add(needTarget);
                        targets.Clear();
                    }
                    
                }
                else
                {
                    if (!IsTargetInRange(target))
                    {
                        enemyBase = runtimeModel.RoMap.Bases[
                        IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId];
                        targets.Add(enemyBase);
                    }
                }
            }   

            return result;
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