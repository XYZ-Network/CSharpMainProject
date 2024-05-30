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
        private List<Vector2Int> _targetsOutOfReach = new List<Vector2Int>();

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
            if (_targetsOutOfReach.Count <= 0 || IsTargetInRange(_targetsOutOfReach[0])) return unit.Pos;
            return unit.Pos.CalcNextStepTowards(_targetsOutOfReach[0]);
        }

        protected override List<Vector2Int> SelectTargets()
        {
            ///////////////////////////////////////
            // Homework 1.4 (1st block, 4rd module)
            ///////////////////////////////////////

            List<Vector2Int> result = new List<Vector2Int>(GetAllTargets());
            _targetsOutOfReach.Clear();
            if (result.Count > 0)
            {
                Vector2Int outTarget = new();
                float minDist = float.MaxValue;
                float nowDist = 0;

                foreach (Vector2Int nowTarget in result)
                {
                    nowDist = DistanceToOwnBase(nowTarget);
                    if(nowDist < minDist)
                    {
                        minDist = nowDist;
                        outTarget = nowTarget;
                    }
                }
                _targetsOutOfReach.Add(outTarget);

                result.Clear();
                if (IsTargetInRange(outTarget)) result.Add(outTarget);
                else _targetsOutOfReach.Add(outTarget);
            }
            else
            {
                result.Add(runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId]);
                _targetsOutOfReach.Add(runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId]);
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