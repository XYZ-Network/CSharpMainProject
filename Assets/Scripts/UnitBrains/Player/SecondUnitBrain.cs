using System;
using System.Collections.Generic;
using System.Linq;
using Codice.CM.Client.Differences.Merge;
using Model;
using Model.Runtime.Projectiles;
using PlasticGui;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UIElements;
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
        private List<Vector2Int> UnreacheableTargets = new();

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            if (GetTemperature() < overheatTemperature)
            {
                for (int i = 1; i <= GetTemperature() + 1; i++)
                {
                    var projectile = CreateProjectile(forTarget);
                    AddProjectileToList(projectile, intoList);
                }
                IncreaseTemperature();
            }
        }

        public override Vector2Int GetNextStep()
        {
            if (UnreacheableTargets.Count == 0)
                return unit.Pos;
            else if (GetReachableTargets().Contains(UnreacheableTargets[0]))
                return unit.Pos;
            else
                return unit.Pos.CalcNextStepTowards(UnreacheableTargets[0]);
        }

        protected override List<Vector2Int> SelectTargets()
        {
            UnreacheableTargets.Clear();
            List<Vector2Int> result = GetAllTargets().ToList();
            if (result.Count > 1)
            {
                Vector2Int nearestTarget = new();
                float min = float.MaxValue;
                foreach (var target in result)
                {
                    float distance = DistanceToOwnBase(target);
                    if (distance < min)
                    {
                        min = distance;
                        nearestTarget = target;
                    }
                }
                result.Clear();
                UnreacheableTargets.Add(nearestTarget);
                if (GetReachableTargets().Contains(nearestTarget))
                    result.Add(nearestTarget);
            }
            else
            {
                result.Clear();
                Vector2Int baseCoordinates = runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId];
                UnreacheableTargets.Add(baseCoordinates);
                if (IsTargetInRange(baseCoordinates))
                    result.Add(baseCoordinates);
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