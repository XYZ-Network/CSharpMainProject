using System.Collections.Generic;
using System.Linq;
using System.Net;
using Model;
using Model.Runtime.Projectiles;
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

        List<Vector2Int> targetsInRange = new List<Vector2Int>(); //цели в зоне досягаемости 
        List<Vector2Int> targetsOutOfRange = new List<Vector2Int>(); //цели out of reach

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            ///////////////////////////////////////
            // Homework 1.3 (1st block, 3rd module)
            ///////////////////////////////////////

            //1.3.2 Weapon load increase//

            int projectileCount = GetTemperature();

            for (int i = 0; i < projectileCount; i++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }

            //1.3.1 Weapon heating up//

            float currentTemperature = GetTemperature();

            if (currentTemperature < overheatTemperature)
            {
                IncreaseTemperature();
            }

            else
            {
                Debug.Log("Overheated");
                return;
            }

            ///////////////////////////////////////
        }

        public override Vector2Int GetNextStep()
        {
            var unitPos = unit.Pos;

            if (targetsInRange.Count > 0) //если в зоне досягаемости есть цели
            {
                return unitPos; //возвращаем позицию юнита
            }
 
            else if (targetsOutOfRange.Count > 0)
            {
                Vector2Int targetPos = targetsOutOfRange[0];
                var newUnitPos = unitPos.CalcNextStepTowards(targetPos);
                return newUnitPos;
            }
            
            return unitPos; //во всех остальных случаях (нет целей) возвращаем позицию юнита
        }

        protected override List<Vector2Int> SelectTargets()
        {
           
            List<Vector2Int> allTargets = GetAllTargets().ToList(); //все цели
           
            var enemyBase = runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId]; //база врага

            targetsInRange.Clear();
            targetsOutOfRange.Clear();

            if (allTargets.Any())
            {
                foreach (Vector2Int target in allTargets)
                {
                    if (IsTargetInRange(target))
                    {
                        targetsInRange.Add(target);
                    }
                    else
                    {
                        targetsOutOfRange.Add(target);
                    }
                }
            }
            else
            {
                Debug.Log("Нет целей.");
            }

            bool enemyBaseIsInRange = targetsInRange.Contains(enemyBase);

            List<Vector2Int> result = new List<Vector2Int>(targetsInRange);
            
            float minDist = float.MaxValue;
            Vector2Int criticalTarget = Vector2Int.zero;

            foreach (Vector2Int target in result)
            {
                if (minDist > DistanceToOwnBase(target))
                {
                    criticalTarget = target;
                    minDist = DistanceToOwnBase(target);
                }
            }

            if (enemyBaseIsInRange)
                result.Add(enemyBase);
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