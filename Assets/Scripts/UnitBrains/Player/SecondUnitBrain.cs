using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Model.Runtime;
using Model.Runtime.Projectiles;
using UnityEngine;

namespace UnitBrains.Player
{
    public class SecondUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Cobra Commando";
        private const float OverheatTemperature =4f;
        private const float OverheatCooldown = 0f;
        private float _temperature = 0f;
        private float _cooldownTime = 0f;
        private bool _overheated;
        
        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            ///////////////////////////////////////
            // Homework 1.3 (1st block, 3rd module)
            ///////////////////////////////////////    
            ///
            float temperat = GetTemperature();

            if (temperat >= overheatTemperature) return;



            for (float strel = 1f; strel <= temperat; strel += 1f)
            {
               
                    var projectile = CreateProjectile(forTarget);
                    AddProjectileToList(projectile, intoList);

            }

            IncreaseTemperature();
        }

        public override Vector2Int GetNextStep()
        {
            return base.GetNextStep();
        }

        protected override List<Vector2Int> SelectTargets()
        {          
            List<Vector2Int> result = GetReachableTargets();

            float min= float.MaxValue;
            Vector2Int near = Vector2Int.zero;


            if (result.Count == 0)
            {
                return result;
            }
            foreach (Vector2Int i in result)
            {
                float Distance = DistanceToOwnBase(i);


                if (Distance < min) 
                {
                    min = Distance;
                    near = i;
                }
               

            }

            result.Clear();
            result.Add(near);
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