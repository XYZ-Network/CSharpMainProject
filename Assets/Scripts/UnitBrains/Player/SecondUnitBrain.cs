using System.Collections.Generic;
using System.Linq;
using Model;
using Model.Runtime.Projectiles;
using UnityEngine;
using Utilities;

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
        private List<Vector2Int> TargetOutRangeUnit = new();

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            ///////////////////////////////////////
            // Homework 1.3 (1st block, 3rd module)
           
            if (GetTemperature() <= OverheatTemperature)
            {
                for (int i = 0; i < GetTemperature(); ++i)
                {
                    var projectile = CreateProjectile(forTarget);
                    AddProjectileToList(projectile, intoList);
                }
            }
            else
            {
                return;
            }

            IncreaseTemperature();
        }

        public override Vector2Int GetNextStep()
        {
            if (TargetOutRangeUnit.Count > 0)
            {
                Vector2Int position = unit.Pos;
                Vector2Int nextPosition = TargetOutRangeUnit[0];
                return position.CalcNextStepTowards(nextPosition);
            }

            return SelectTargets()[0];
        }

        protected override List<Vector2Int> SelectTargets()
        {
            ///////////////////////////////////////
            // Homework 1.4 (1st block, 4rd module)
            ///////////////////////////////////////
            List<Vector2Int> result = GetAllTargets().ToList();
            Vector2Int DangerTarget = FindTarget(result);

            if (IsTargetInRange(DangerTarget)) 
            {
                Debug.Log("Замечен враг");
                result.Clear();
                result.Add(DangerTarget);
                return result;
            }
            else if (!IsTargetInRange(DangerTarget))
            {
                Debug.Log("Враг вне поля зрения");
                TargetOutRangeUnit.Clear();
                TargetOutRangeUnit.Add(DangerTarget);
                result.Clear();
            }
            else
            {
                TargetOutRangeUnit.Clear();
                TargetOutRangeUnit.Add(runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId]);
            }

            return result;
        }

        public Vector2Int FindTarget(List<Vector2Int> results)
        {
            Vector2Int DangerTarget = default;
            var maxTargetDistance = float.MaxValue;

            if (results != null)
            {
                foreach (var target in results)
                {
                    if (DistanceToOwnBase(target) < maxTargetDistance)
                    {
                        DangerTarget = target;
                        maxTargetDistance = DistanceToOwnBase(target);
                    }
                }
            }

            return DangerTarget;
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