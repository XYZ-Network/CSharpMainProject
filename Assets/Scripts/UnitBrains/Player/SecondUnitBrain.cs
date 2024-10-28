using System.Collections.Generic;
using System.Linq;
using GluonGui.Dialog;
using Model;
using Model.Runtime.Projectiles;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Utilities;

namespace UnitBrains.Player
{
    public class SecondUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Cobra Commando";
        private const float OverheatTemperature = 3f;
        private List<Vector2Int> targetsOutRangeUnit = new();
        private const float OverheatCooldown = 2f;
        private float _temperature = 0f;
        private float _cooldownTime = 0f;
        private bool _overheated;

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;

            if (GetTemperature() >= overheatTemperature)
                return;

            IncreaseTemperature();

            for (int i = 0; i < GetTemperature(); i++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }
        }

        public override Vector2Int GetNextStep()
        {
            if (targetsOutRangeUnit.Count > 0)
            {
                Vector2Int position = unit.Pos;
                Vector2Int nextPosition = targetsOutRangeUnit[0];
                return position.CalcNextStepTowards(nextPosition);
            }

            return SelectTargets()[0];
        }

        protected override List<Vector2Int> SelectTargets()
        {
            List<Vector2Int> targets = GetAllTargets().ToList();
            Vector2Int dangerTarget = FindNearestTarget(targets);

            if (IsTargetInRange(dangerTarget))
            {
                Debug.Log("ВРАГ ОБНАРУЖЕН");
                targets.Clear();
                targets.Add(dangerTarget);
                return targets;
            }
            else if(!IsTargetInRange(dangerTarget))
            {
                Debug.Log("ВРАГ ВНЕ ДОСИГАЕМОСТИ");
                targetsOutRangeUnit.Clear();
                targetsOutRangeUnit.Add(dangerTarget);
                targets.Clear();
            }
            else
            {
                targetsOutRangeUnit.Clear();
                targetsOutRangeUnit.Add(runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId]);
            }

            
            return targets;
        }

        public Vector2Int FindNearestTarget(List<Vector2Int> targets)
        {
            Vector2Int dangerTarget = default;
            var maxTargetDistanse = float.MaxValue;

            if (targets != null)
            {
                foreach (var target in targets)
                {
                    if (DistanceToOwnBase(target) < maxTargetDistanse)
                    {
                        dangerTarget = target;
                        maxTargetDistanse = DistanceToOwnBase(target);
                    }
                }
            }

            return dangerTarget;
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