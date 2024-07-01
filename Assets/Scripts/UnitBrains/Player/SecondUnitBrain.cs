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

        private List<Vector2Int> _enemyOutOfRange = new List<Vector2Int>(); //List for save Dangerous Units.

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            int currentTemeperature = GetTemperature();
            ///////////////////////////////////////
            // Homework 1.3 (1st block, 3rd module)
            ///////////////////////////////////////           
            if (currentTemeperature >= overheatTemperature)
            {
                return;
            }
            else
            {
                for (int i = 0; i <= currentTemeperature; i++)
                {
                    var projectile = CreateProjectile(forTarget);
                    AddProjectileToList(projectile, intoList);
                    //Debug.Log(_temperature + 1);
                }
                IncreaseTemperature();
            }
            ///////////////////////////////////////
        }

        public override Vector2Int GetNextStep()
        {
            Vector2Int target = Vector2Int.zero;
            target = _enemyOutOfRange.Any() ? _enemyOutOfRange[0] : unit.Pos;

            return IsTargetInRange(target) ? unit.Pos : unit.Pos.CalcNextStepTowards(target);
        }

        protected override List<Vector2Int> SelectTargets()
        {
            ///////////////////////////////////////
            // Homework 1.4 (1st block, 4rd module)
            ///////////////////////////////////////
            List<Vector2Int> result = new List<Vector2Int>(GetAllTargets());
            _enemyOutOfRange.Clear();

            if (result.Count > 0)
            {
                Vector2Int closestTarget = new Vector2Int();
                float closestDistance = float.MaxValue;
                
                foreach (var r in result)
                {
                    float distance = DistanceToOwnBase(r);

                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestTarget = r;
                    }
                }
                
                _enemyOutOfRange.Add(closestTarget);

                if (IsTargetInRange(closestTarget))
                {
                    result.Add(closestTarget);
                }
            }

            else
            {
                result.Add(runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId]);
            }

            return result;
            /*Vector2Int closestTarget = Vector2Int.zero;
            
            float closestDistance = float.MaxValue;

            foreach (var r in result)
            {
                float distance = DistanceToOwnBase(r);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTarget = r;
                }
            }

            while (result.Count > 1)
            {
                result.Clear();
                if (closestDistance < float.MaxValue)
                    result.Add(closestTarget);
                result.RemoveAt(result.Count - 1);
            }*/
            //return result;
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