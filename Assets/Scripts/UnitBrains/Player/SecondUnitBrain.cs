using System.Collections.Generic;
using Model;
using Model.Runtime.Projectiles;
using Unity.VisualScripting;
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

        List<Vector2Int> targetsOutOfRange = new List<Vector2Int>();

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;

            float currentTemperature = GetTemperature();

            if (currentTemperature >= overheatTemperature)
            {
                return;
            }

            IncreaseTemperature();

            for (int i = 0; i <= currentTemperature; i++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }
        }

        public override Vector2Int GetNextStep()
        {
            Vector2Int currentTarget = targetsOutOfRange.Count > 0 ? targetsOutOfRange[0] : unit.Pos;

            if (IsTargetInRange(currentTarget))
            {
                return unit.Pos;
            } else
            {
                return unit.Pos.CalcNextStepTowards(currentTarget);
            }
        }

        protected override List<Vector2Int> SelectTargets()
        {
            List<Vector2Int> result = new List<Vector2Int>();

            foreach (Vector2Int i in GetAllTargets())
            {
                result.Add(i);
            }

            float closestDistance = float.MaxValue;
            Vector2Int closestTarget = Vector2Int.zero;

            if (result.Count >= 1)
            {
                foreach (Vector2Int target in result)
                {
                    float distance = DistanceToOwnBase(target);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestTarget = target;
                    }
                }

                targetsOutOfRange.Clear();
                targetsOutOfRange.Add(closestTarget);

                if (IsTargetInRange(closestTarget))
                {
                    result.Clear();
                    result.Add(closestTarget);                  
                } else
                {
                    int playerId = IsPlayerUnitBrain ? RuntimeModel.PlayerId : RuntimeModel.BotPlayerId;
                    Vector2Int enemyBase = runtimeModel.RoMap.Bases[playerId];
                    targetsOutOfRange.Add(enemyBase);
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