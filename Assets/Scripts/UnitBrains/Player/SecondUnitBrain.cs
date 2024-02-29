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
        List<Vector2Int> unreachableTargets = new List<Vector2Int>();

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            ///////////////////////////////////////
            float temp = GetTemperature();
            if (temp >= overheatTemperature)
            {
                return;
            }
            IncreaseTemperature();
            for (int i = 0; i <= temp; i++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
                Debug.Log("ВЫСТРЕЛ");
                //Debug.Log("Выстрел сработал по координатам врага = " + forTarget.ToString());
            }
            ///////////////////////////////////////
        }

        public override Vector2Int GetNextStep()
        {
            Vector2Int target = unreachableTargets.Count > 0 ? unreachableTargets[0] : unit.Pos;

            if (IsTargetInRange(target))
            {
                Debug.Log($"GetNextStep /// Таргет в пределах досигаемости");
                return unit.Pos;
            }
            else
            {
                Debug.Log($"GetNextStep /// Нужно двигаться к следующей цели");
                return unit.Pos.CalcNextStepTowards(target);
            }      
        }

        protected override List<Vector2Int> SelectTargets() 
        {
            List<Vector2Int> result = new();
            float minDistance = float.MaxValue;
            Vector2Int nearestTarget = Vector2Int.zero;
            Vector2Int enemyBase = runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId];

            foreach (var target in GetAllTargets())
            {
                float distanceToCurrentTarget = DistanceToOwnBase(target);

                if (minDistance >= distanceToCurrentTarget)
                {
                    minDistance = distanceToCurrentTarget;
                    nearestTarget = target;

                    if (IsTargetInRange(nearestTarget))
                    {
                        result.Add(nearestTarget);
                        unreachableTargets.Clear();
                        Debug.Log("SelectTargets /// Таргет на расстоянии выстрела.");
                    }
                    else
                    {
                        unreachableTargets.Add(nearestTarget);
                        Debug.Log("SelectTargets /// Таргет слишком далеко.");
                    }
                }
                else
                {
                    if (!IsTargetInRange(nearestTarget)) 
                    { 
                        unreachableTargets.Add(enemyBase);
                        Debug.Log("База противника добавлена в таргет");
                    }
                }
            }

            return result;
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