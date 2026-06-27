using Model;
using Model.Runtime.Projectiles;
using System.Collections.Generic;
using System.Linq;
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

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            ///////////////////////////////////////
            // Homework 1.3 (1st block, 3rd module)
            ///////////////////////////////////////     

            int temperature = GetTemperature();
            if (temperature >= overheatTemperature)
            {
                return;
            }

            int projectileAmount = temperature + 1;
            for (int i = 0; i < projectileAmount; i++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }

            IncreaseTemperature();
            ///////////////////////////////////////
        }

        public override Vector2Int GetNextStep()
        {
            List<Vector2Int> targets = SelectTargets();
            if (targets.Count > 0)
            {
                Vector2Int firstTarget = targets[0];
                if (IsTargetInRange(firstTarget))
                {
                    return unit.Pos;
                }
                return unit.Pos.CalcNextStepTowards(firstTarget);
            }
            return unit.Pos;
        }

        protected override List<Vector2Int> SelectTargets()
        {
            ///////////////////////////////////////
            // Homework 1.4 (1st block, 4rd module)
            ///////////////////////////////////////
            List<Vector2Int> allTargets = GetAllTargets().ToList();
            List<Vector2Int> result = new List<Vector2Int>();

            if (allTargets.Count <= 0)
            {
                Vector2Int enemyBase = runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId];
                result.Add(enemyBase);
                return result;
            }

            float minDistance = float.MaxValue;
            Vector2Int finalTarget = allTargets[0];

            foreach (var target in allTargets)
            {
                float targetDist = DistanceToOwnBase(target);
                if (targetDist < minDistance)
                {
                    minDistance = targetDist;
                    finalTarget = target;
                }
            }

            result.Add(finalTarget);

            return result;
            ///////////////////////////////////////
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