using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net;
using Model;
using Model.Runtime.Projectiles;
using Unity.VisualScripting.YamlDotNet.Serialization;
using UnityEngine;
using UnityEngine.Video;
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
        public List<Vector2Int> Outreachable = new List<Vector2Int>();
        
        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            ///////////////////////////////////////
            // Homework 1.3 (1st block, 3rd module)
            ///////////////////////////////////////
            if (GetTemperature() <= overheatTemperature)
            {
                IncreaseTemperature();
            }
            for (int w = GetTemperature();w <= overheatTemperature; w++ )
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);

                if (w > GetTemperature())
                {
                    break;
                }
            }

            ///////////////////////////////////////
        }

        public override Vector2Int GetNextStep()
        {
            Vector2Int target = Outreachable[0];
            if (Outreachable.Count > 0 && !IsTargetInRange(target))
            {
                return unit.Pos.CalcNextStepTowards(target);
            }
            else
            {
                return unit.Pos;
            }

        }

        protected override List<Vector2Int> SelectTargets()
        {
            ///////////////////////////////////////
            // Homework 1.4 (1st block, 4rd module)
            ///////////////////////////////////////

            List<Vector2Int> result = new List<Vector2Int>();
            float min = float.MaxValue;
            Vector2Int NearEnemy = Vector2Int.zero;

            foreach (var target in GetAllTargets())
            {
                if (min >= DistanceToOwnBase(target))
                {
                    min = DistanceToOwnBase(target);
                    NearEnemy = target;
                }
            }

            if (IsTargetInRange(NearEnemy))
            {
                result.Add(NearEnemy);
            }
            else
            {
                Outreachable.Add(NearEnemy);
            }

            if (result.Count == 0 && Outreachable.Count == 0)
            {
                if (IsTargetInRange(runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId]))
                {
                    result.Add(runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId]);
                    return result;
                }
                Outreachable.Add(runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId]);
                return result;
            }
            else
            {
                return result;
            }
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