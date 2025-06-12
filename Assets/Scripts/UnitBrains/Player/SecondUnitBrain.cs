using System;
using System.Collections.Generic;
using System.Linq;
using Codice.CM.Client.Differences.Merge;
using Model;
using Model.Runtime;
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
            else
                return unit.Pos.CalcNextStepTowards(UnreacheableTargets[0]);
        }

        protected override List<Vector2Int> SelectTargets()
        {
            int maxTargets = 3;

            UnreacheableTargets.Clear();
            List<Vector2Int> result = GetAllTargets().ToList();
            SortByDistanceToOwnBase(result);
            Vector2Int mainTarget = new();

            if (result.Count > 1)
                for (int i = 0; i <= result.Count; i++)
                {
                    int index;
                    if (result.Count > maxTargets)
                        index = unit.ID % maxTargets;
                    else
                        index = unit.ID % result.Count;
                    mainTarget = result[index];
                }
            else
                mainTarget = runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId];
            result.Clear();
            if (IsTargetInRange(mainTarget))
                result.Add(mainTarget);
            else
                UnreacheableTargets.Add(mainTarget);
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