using System;
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
        private List<Vector2Int> ListTarget =new List<Vector2Int>();
        
        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {       
            float overheatTemperature = OverheatTemperature;

            if (GetTemperature() >= overheatTemperature)
            {
                return;
            }

            for (int i = 0; i < GetTemperature() + 1f; i++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }

            IncreaseTemperature();



        }

        public override Vector2Int GetNextStep()
        {
            if (ListTarget.Count> 0)
            {
                return unit.Pos.CalcNextStepTowards(ListTarget[0]);
            }
            return unit.Pos;
        }

        protected override List<Vector2Int> SelectTargets()
        {
            List<Vector2Int> result = GetAllTargets().ToList();
            List<Vector2Int> NextStepList = new List<Vector2Int>();
            float MinDistances = float.MaxValue;
            if (result.Count > 0) 
            { 
           
            
            float Mindistance = int.MaxValue;
                Vector2Int MinDistancesObject = result[0];
                foreach (Vector2Int target in result)
                {
                    float TmpDistansToBase = DistanceToOwnBase(target);
                    if (TmpDistansToBase< Mindistance)
                {
                        MinDistances = TmpDistansToBase;
                        MinDistancesObject = target;

                    }
            }
            //Очищаем список
            result.Clear();
        ListTarget.Clear();
                if (IsTargetInRange(MinDistancesObject))
                    result.Add(MinDistancesObject);
                else
                    ListTarget.Add(MinDistancesObject);

            }
            else
            {
                ListTarget.Clear();
                Vector2Int enemyBase = runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.PlayerId : RuntimeModel.BotPlayerId];
                ListTarget.Add(enemyBase);

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