using System.Collections.Generic;
using System.Linq;
using Model;
using Model.Runtime.Projectiles;
using Unity.VisualScripting;
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
        private List<Vector2Int> DangerTargetsOutOfRange = new List<Vector2Int>();

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            ///////////////////////////////////////
            // Homework 1.3 (1st block, 3rd module)
            int currentTemperature = GetTemperature();

            if (currentTemperature >= overheatTemperature)
            {
                return;
            }

            ///////////////////////////////////////
            
            for (int i = 0; i < currentTemperature + 1; i++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }

            IncreaseTemperature();
            ///////////////////////////////////////
        }

        public override Vector2Int GetNextStep()
        {
            if (DangerTargetsOutOfRange.Count > 0)
            {
                Vector2Int currentTarget = DangerTargetsOutOfRange[0];

                if (IsTargetInRange(currentTarget))
                {
                    return unit.Pos;
                }
                else
                {
                    return unit.Pos.CalcNextStepTowards(currentTarget);
                }
            }
            else
            {
                return unit.Pos;
            }
        }

        protected override List<Vector2Int> SelectTargets()
        {

            List<Vector2Int> result = new List<Vector2Int>();
            DangerTargetsOutOfRange = GetAllTargets().ToList();

            if (DangerTargetsOutOfRange.Count > 0)
            {
                float minDistance = float.MaxValue;
                Vector2Int closestTarget = Vector2Int.zero;

                foreach (Vector2Int target in DangerTargetsOutOfRange)
                {
                    if (DistanceToOwnBase(target) < minDistance)
                    {
                        minDistance = DistanceToOwnBase(target);
                        closestTarget = target;
                    }
                }

                DangerTargetsOutOfRange.Clear();
                DangerTargetsOutOfRange.Add(closestTarget);

                if (minDistance != float.MaxValue)
                {
                    if (IsTargetInRange(closestTarget))
                    {
                        result.Clear();
                        result.Add(closestTarget);
                    }
                }
            }
            else
            {
                Vector2Int enemyBase = runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId];
                DangerTargetsOutOfRange.Add(enemyBase);
            }

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