using System;
using System.Collections.Generic;
using System.Linq;
using Model;
using Model.Runtime.Projectiles;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
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

        public List<Vector2Int> OutOfRange = new();

        
        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            float t = GetTemperature();
            IncreaseTemperature();
            if (t >= overheatTemperature)
            {
                return;
            }

            for (float i = GetTemperature(); i <= 3 && i > 0; i--)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }
            IncreaseTemperature();

        }

        public override Vector2Int GetNextStep()
        {
            Vector2Int target = OutOfRange[0];
            Vector2Int nextPosition = Vector2Int.right;
            if (OutOfRange.Count > 0 && !IsTargetInRange(target))
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
            List<Vector2Int> result = GetReachableTargets();
            Vector2Int targetEn = Vector2Int.zero;

            OutOfRange.Clear();

            float minD = float.MaxValue;


            foreach (var target in GetAllTargets())
            {
                float dis = DistanceToOwnBase(target);
                if (dis < minD)
                {
                    minD = dis;
                    targetEn = target;
                }
            }


            if (IsTargetInRange(targetEn))
            {
                result.Add(targetEn);
            }
            else
            {
                OutOfRange.Add(targetEn);
            }


            if (result.Count == 0 && OutOfRange.Count == 0)
            {
                var target = runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId];
                if (IsTargetInRange(target))
                {
                    result.Add(target);
                    return result;
                }

                OutOfRange.Add(target);
                return result;
            }
            else
            {
                return result;
            }
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