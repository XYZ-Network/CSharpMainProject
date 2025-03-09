using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Model;
using Model.Runtime.Projectiles;
using UnityEditor.Experimental.GraphView;
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
        private List<Vector2Int> targetToGo = new();
        private IEnumerable<Vector2Int> resultToGo;
        private int x = 0;

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            ///////////////////////////////////////
            // Homework 1.3 (1st block, 3rd module)
            ///////////////////////////////////////     
            float getTemperature = GetTemperature();
            if (getTemperature < overheatTemperature)
            {
                for (int i = 0; i <= getTemperature; i++)
                {
                    var projectile = CreateProjectile(forTarget);
                    AddProjectileToList(projectile, intoList);
                }
                IncreaseTemperature();
            }
            ///////////////////////////////////////
        }

        public override Vector2Int GetNextStep()
        {
            
            //если цель в области атаки

            if (!targetToGo.Any() || IsTargetInRange(targetToGo[0]))// если целей нет или в области атаки
            {
                return this.unit.Pos;
            }
            //если цель вне области атаки
            //Debug.Log(unit.Pos.CalcNextStepTowards(targetToGo));
            //Debug.Log("GetNextStep" + x);
            //x++;
            return unit.Pos.CalcNextStepTowards(targetToGo.First());
        }

        protected override List<Vector2Int> SelectTargets()
        {
            ///////////////////////////////////////
            // Homework 1.4 (1st block, 4rd module)
            ///////////////////////////////////////
            List<Vector2Int> result = GetReachableTargets();
            resultToGo = GetAllTargets();
            Vector2Int bestTarget = Vector2Int.zero;
            Vector2Int asdtargetInRange = Vector2Int.zero;
            float dist = float.MaxValue;
            //float distInRange = float.MaxValue;
            //List<Vector2Int> dangerToGo = new List<Vector2Int>();
            //while (result.Count > 1)

            if (resultToGo.Any())
            {
                //Debug.Log("зашли в resultToGo.Any()");
                foreach (var target in resultToGo)
                {
                    if (DistanceToOwnBase(target) < dist)
                    {
                        dist = DistanceToOwnBase(target);
                        bestTarget = target;
                        //Debug.Log("targetToGo = " + targetToGo);
                    }
                }


            }
            else
            {
                Debug.Log("ищем базу");
                result.Clear();
                bestTarget = (runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId]);
                result.Add(bestTarget);
                return result;
            }

            if (!IsTargetInRange(bestTarget))
            {
                targetToGo.Clear();
                targetToGo.Add(bestTarget);
                result.Clear();
                return result;
            }

            result.Clear();
            result.Add(bestTarget);
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