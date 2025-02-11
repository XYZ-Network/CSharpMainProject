using System.Collections.Generic;
using System.Linq;
using Codice.Client.BaseCommands.WkStatus.Printers;
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
        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            if(GetTemperature() >= overheatTemperature) 
            {
                return;
            }
            
            for (int a = 0; a <= GetTemperature(); a++) 
                {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
                }
            IncreaseTemperature();
            ///////////////////////////////////////
        }
        private List<Vector2Int> TargetList = new List<Vector2Int>();
        public override Vector2Int GetNextStep()
        {
            if (TargetList == null)
            {
                return Vector2Int.zero;
            }
            return base.GetNextStep();
        }

        protected override List<Vector2Int> SelectTargets()

        {
            //////////////////////DistanceToOwnBase/////////////////
        float min = float.MaxValue;
        List<Vector2Int> result = GetAllTargets().ToList();
        List<Vector2Int> nextStep = new List<Vector2Int>();

            if (result.Count > 0)
            {
                Vector2Int tar = Vector2Int.zero;
                foreach (Vector2Int target in result)
                {
                    float dis = DistanceToOwnBase(target);
                    if (dis < min)
                    {
                        min = dis;
                        tar = target;

                    }
                }

                result.Clear();
                TargetList.Clear();
                if (IsTargetInRange(tar))
                    result.Add(tar);
                else
                    TargetList.Add(tar);
            }
            else
            {
                TargetList.Clear();
                Vector2Int enemyBase = runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.PlayerId : RuntimeModel.BotPlayerId];
                TargetList.Add(enemyBase);
            }
                return result;
        }
        /// /////////////////////////////////////////////////////////////
     
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