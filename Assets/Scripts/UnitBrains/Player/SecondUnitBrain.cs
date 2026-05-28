using Model;
using Model.Runtime.Projectiles;
using System.Collections.Generic;
using System.Linq;
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
        private List<Vector2Int> UnreachableTargets = new List<Vector2Int>();

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;

            if (GetTemperature() >= overheatTemperature)  //Checking for overheating
            {
                return;
            }

            int projectileCount = (int)( _temperature + 1);  //Calculating the number of shells
            
            for (int i = 0; i < projectileCount; i++)  //Creating projectiles in a cycle
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }

            IncreaseTemperature();  //Temperature rise
        }

        public override Vector2Int GetNextStep()
        {
            if (UnreachableTargets.Count == 0 || GetReachableTargets().Contains(UnreachableTargets[0]))
                return unit.Pos;
            else
                return unit.Pos.CalcNextStepTowards(UnreachableTargets[0]);
        }

        protected override List<Vector2Int> SelectTargets()
        {
            UnreachableTargets.Clear();
            List<Vector2Int> result = GetAllTargets().ToList();
            if (result.Count > 1)
            {
                float min = float.MaxValue;
                Vector2Int minResult = new Vector2Int();
                foreach (Vector2Int res in result)
                {
                    if (DistanceToOwnBase(res) < min)
                    {
                        min = DistanceToOwnBase(res);
                        minResult = res;
                    }

                }
                result.Clear();
                UnreachableTargets.Add(minResult);
                if (GetReachableTargets().Contains(minResult))
                    result.Add(minResult);
            }
            else
            {
                result.Clear();
                var enemyBase = runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId];
                UnreachableTargets.Add(enemyBase);
                result.Add(enemyBase);
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