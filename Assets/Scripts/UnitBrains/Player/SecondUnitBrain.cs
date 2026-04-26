using Model;
using Model.Runtime.Projectiles;
using System.Collections.Generic;
using System.Linq;

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
            ///////////////////////////////////////
            // Homework 1.3 (1st block, 3rd module)
            ///////////////////////////////////////           
            if (GetTemperature() >= overheatTemperature)
            {
                return;
            }

            float i = 0f;

            while (i <= _temperature)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
                i++;
            }
            ///////////////////////////////////////
            IncreaseTemperature();
        }

        public override Vector2Int GetNextStep()
        {
            return base.GetNextStep();

            //Vector2Int position = Vector2Int.zero;
            //Vector2Int nexPosition = Vector2Int.right;
            //return position.CalcNextStepTowards(nexPosition);

        }




        protected override List<Vector2Int> SelectTargets()
        {

            ///////////////////////////////////////
            // Homework 1.4 (1st block, 4rd module)
            /////////////////////////////////////
            //List<Vector2Int> result = (List<Vector2Int>)GetAllTargets();
            List<Vector2Int> result = GetReachableTargets();
            float distance = float.MaxValue;


            if (result.Any())
            {
                Vector2Int dis = new Vector2Int();

                foreach (var item in result)
                {
                    if (DistanceToOwnBase(item) < distance)
                    {
                        distance = DistanceToOwnBase(item);
                        dis = item;
                    }
                }
                if (result.Contains(dis))
                {

                }
                result.Clear();
                result.Add(dis);
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