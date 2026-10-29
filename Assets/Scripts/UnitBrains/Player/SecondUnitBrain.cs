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

        private List<Vector2Int> outOfReachTargets = new List<Vector2Int>(); // коллекция целей вне досягаемости

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            //
            float overheatTemperature = OverheatTemperature;
            float currentTemperature = GetTemperature();

            var projectile = CreateProjectile(forTarget);
           

            int projectilesCount = (int)currentTemperature;

            if (currentTemperature >= overheatTemperature)
            {
                return;
            }

            if (projectilesCount <= 0)
            {
                projectilesCount = 1;
            }

            for (int i = 0; i < projectilesCount; i++)
            {
                projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }

            
            IncreaseTemperature();
        }

        public override Vector2Int GetNextStep()
        {
            
            Vector2Int currentPosition = unit.Pos; // текущая позиция

            
            List<Vector2Int> targets = SelectTargets(); // Заправшиваем список целей
            Vector2Int targetPosition = Vector2Int.zero;
            bool hasTarget = false;

            
            if (targets.Count > 0)
            {
                targetPosition = targets[0];
                hasTarget = true;
            }
            else if (outOfReachTargets.Count > 0)
            {
                targetPosition = outOfReachTargets[0];
                hasTarget = true;
            }

            
            if (!hasTarget) // если нет целей
            {
                return currentPosition;
            }

            
            if (targets.Count > 0 && IsTargetInRange(targetPosition)) // если в цели отаки стоим и атакуем
            {
                return currentPosition;
            }

            
            return currentPosition.CalcNextStepTowards(targetPosition); // если нет движемся к ней
        }

        protected override List<Vector2Int> SelectTargets()
        {
            ///////////////////////////////////////
            // Homework 1.4 (1st block, 4rd module)
            ///////////////////////////////////////
            outOfReachTargets.Clear();

           
            IEnumerable<Vector2Int> allTargetsEnum = GetAllTargets(); // получение цели на карте
            List<Vector2Int> result = new List<Vector2Int>();

          
            List<Vector2Int> allTarget = allTargetsEnum    // если список не ноль добавляем иначе пустой список
                != null ? allTargetsEnum.ToList() 
                : new List<Vector2Int>();

           
            if (allTarget.Count > 0) // если в целях есть ищем самую близкую к базе
            {
                Vector2Int closestTarget = allTarget[0];
                float minDistance = DistanceToOwnBase(closestTarget);

                for (int i = 1; i < allTarget.Count; i++)
                {
                    float dist = DistanceToOwnBase(allTarget[i]);
                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        closestTarget = allTarget[i];
                    }
                }

                
                List<Vector2Int> reachableTargets = GetReachableTargets(); // проверка зоны досягаесмости

                if (reachableTargets != null && reachableTargets.Contains(closestTarget))
                {
                   
                    result.Add(closestTarget); // если да то добавляем  в список result
                }
                else
                {
                   
                    outOfReachTargets.Add(closestTarget); //иначе в список вне досягаемости
                }
            }
            
            else // если нет добовляем базу противника как сказана в инструкции к уроку
            {
                
                int enemyPlayerId = IsPlayerUnitBrain // если юнит игрок  то враг компьютер, иначе - игрок
                    ? RuntimeModel.BotPlayerId 
                    : RuntimeModel.PlayerId;

                
                Vector2Int enemyBase = runtimeModel.RoMap.Bases[enemyPlayerId]; // определение базы противника

               
                result.Add(enemyBase); // добавляю в список
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