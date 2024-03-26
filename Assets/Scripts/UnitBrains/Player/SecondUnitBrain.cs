using System.Collections.Generic;
using Model;
using Model.Runtime.Projectiles;
using Unity.VisualScripting;
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
        private List<Vector2Int> TargetsOutReach = new List<Vector2Int>();

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            int currentTemperature = GetTemperature();

            if (currentTemperature >= OverheatTemperature)
                return;

            for (int i = 0; i <= currentTemperature; i++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }

            IncreaseTemperature();
        }

        public override Vector2Int GetNextStep()
        {
            Vector2Int target = Vector2Int.zero;
            if (TargetsOutReach.Count > 0)
                target = TargetsOutReach[0];
            else
                target = unit.Pos;

            if (IsTargetInRange(target))
                return unit.Pos;
            else
            {
                return unit.Pos.CalcNextStepTowards(target);
            }
        }

        protected override List<Vector2Int> SelectTargets()
        {
            Vector2Int target = Vector2Int.zero;
            float distance = float.MaxValue;
            List<Vector2Int> result = new List<Vector2Int>();
                        
            foreach (var j in result)
            {
                float range = DistanceToOwnBase(j);
                if (distance > range)
                {
                    target = j;
                    distance = range;
                }
            }

            TargetsOutReach.Clear();

            if (distance < float.MaxValue)
            {
                if (IsTargetInRange(target))
                    result.Add(target);
                
                TargetsOutReach.Add(target);
            }
            else
            {
                int enemyBaseId;
                if (IsPlayerUnitBrain)
                    enemyBaseId = RuntimeModel.BotPlayerId;
                else
                    enemyBaseId = RuntimeModel.PlayerId;
                
                Vector2Int enemyBase = runtimeModel.RoMap.Bases[enemyBaseId];
                TargetsOutReach.Add(enemyBase);
            }

            return result;
        }

        public override void Update(float deltaTime, float time)
        {
            if (_overheated)
            {
                _cooldownTime += Time.deltaTime;
                float t = _cooldownTime / (OverheatCooldown / 10);
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
            if (_overheated) return (int)OverheatTemperature;
            else return (int)_temperature;
        }

        private void IncreaseTemperature()
        {
            _temperature += 1f;
            if (_temperature >= OverheatTemperature) _overheated = true;
        }
    }
}

