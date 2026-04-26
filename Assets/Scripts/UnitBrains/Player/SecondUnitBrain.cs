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
        
        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            ///////////////////////////////////////
            // Homework 1.3 (1st block, 3rd module)
            ///////////////////////////////////////
            if (GetTemperature() < overheatTemperature)
            {
                for (int i = 0; i < GetTemperature() + 1; i++)
                {
                    var projectile = CreateProjectile(forTarget);
                    AddProjectileToList(projectile, intoList);
                }
                IncreaseTemperature();
            }
            else
            {
                return;
            }
            ///////////////////////////////////////
        }

        public override Vector2Int GetNextStep()
        {
            Vector2Int position = unit.Pos;
            Vector2Int nextPosition = GetAllTargets().ToList()[0];
            if (IsTargetInRange(nextPosition))
            {
                return position;
            }
            return position.CalcNextStepTowards(nextPosition);
        }

        protected override List<Vector2Int> SelectTargets()
        {
            ///////////////////////////////////////
            // Homework 1.4 (1st block, 4rd module)
            ///////////////////////////////////////
            List<Vector2Int> result = GetAllTargets().ToList();
            List<Vector2Int> BodiesNotInRange = new List<Vector2Int>();

            float minima = float.MaxValue;


            if (result.Count() == 0)
            {
                int enemyId = IsPlayerUnitBrain ? RuntimeModel.PlayerId : RuntimeModel.BotPlayerId;
                result.Clear();
                result.Add(runtimeModel.RoMap.Bases[RuntimeModel.PlayerId]);
                return result;
            }
            if (result.Count > 0)
            {
                Vector2Int MinBody = result[0];
                foreach (Vector2Int Body in result)
                {
                    if (!IsTargetInRange(Body))
                    {
                        BodiesNotInRange.Add(Body);
                    }
                    if (IsTargetInRange(Body) && DistanceToOwnBase(Body) < minima)
                    {
                        MinBody = Body;
                        minima = DistanceToOwnBase(Body);
                    }
                }
                result.Clear();
                result.Add(MinBody);

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