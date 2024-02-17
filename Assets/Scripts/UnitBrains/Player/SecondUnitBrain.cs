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
            return base.GetNextStep();
        }

        protected override List<Vector2Int> SelectTargets()
        {
            ///////////////////////////////////////
            // Homework 1.4 (1st block, 4rd module)
            ///////////////////////////////////////
            List<Vector2Int> result=new List<Vector2Int>();
            foreach (var target in GetAllTargets())
            {
                result.Add(target);
            }

            if (result.Count > 0)
            {



                float blizko = float.MaxValue;
                Vector2Int best = Vector2Int.zero;


                foreach (Vector2Int target in result)
                {
                    float dlinna = DistanceToOwnBase(target);
                    if (dlinna < blizko)
                    {
                        blizko = dlinna;
                        best = target;
                    }
                }
                result.Clear();
                result.Add(best);
                priora.Add(best);
                return result;
                
            }
            else
            {
                List<System.Collections.Generic.IReadOnlyList<UnityEngine.Vector2Int>> cel = new List<IReadOnlyList<Vector2Int>>();
                

                
                cel.Add(runtimeModel.RoMap.Bases);

                return result;
            }
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