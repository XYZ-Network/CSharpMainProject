using System.Collections.Generic;
using Model.Runtime.Projectiles;
using UnityEngine;

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
            // Модуль 3. Задание 1
              while(true)
            {

                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
                GetTemperature();

                if(GetTemperature() >= overheatTemperature )
                {
                    break;
                }
                IncreaseTemperature();

            }
        }

        public override Vector2Int GetNextStep()
        {
            return base.GetNextStep();
        }

        protected override List<Vector2Int> SelectTargets()
        {
            ///////////////////////////////////////
             // Задание 1.4 Киктенко А.А.
             float minDistance = float.MaxValue;
             Vector2 closestTarget = Vector2.right;

             List <Vector2Int> result = GetReachableTargets();
             foreach (var target in result)
             {
              float currentDistance = DistanceToOwnBase(target);

              if (currentDistance < minDistance)
             {
               minDistance = currentDistance;
               closestTarget = target;
              }
            }
           if (closestTarget != null)
           {
     
            result.Clear();
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
