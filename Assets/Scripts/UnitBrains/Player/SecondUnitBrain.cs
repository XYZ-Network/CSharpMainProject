using System.Collections.Generic;
using Model;
using Model.Runtime.Projectiles;
using Unity.VisualScripting;
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
        private const float constantin = 3f;
        private static int chet = 0;
        private float chert = 0; 
        private bool _overheated;
        private List<Vector2Int> priora = new List<Vector2Int>();
        
        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
                float overheatTemperature = OverheatTemperature;
                  float temp = GetTemperature();

                   if (temp > overheatTemperature)
                  {
                    return;
                  }  



            for (int i = 0; i <= temp; i++)

            {
                var projectile = CreateProjectile(forTarget);
            

                AddProjectileToList(projectile, intoList);

            }
            IncreaseTemperature();
        }
        public override Vector2Int GetNextStep()
        {
            Vector2Int target = Vector2Int.zero;


            if (priora.Count > 0)

            {

                target = priora[0];
            }

            else

            {

                target = unit.Pos;
               
            }
            if (IsTargetInRange(target)) return unit.Pos;
            else return unit.Pos.CalcNextStepTowards(target);
        }

        protected override List<Vector2Int> SelectTargets()
        {



            List<Vector2Int> result = new List<Vector2Int>();
            priora.Clear();
            foreach (Vector2Int target in GetAllTargets())
            {

                
                priora.Add(target);
                

            }
            if (priora.Count == 0)
            {
                priora.Add(runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId]);
            }
            SortByDistanceToOwnBase(priora);
            for (chet=0;chet<constantin;chet++)
            {
               Vector2Int n = priora[chet];
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