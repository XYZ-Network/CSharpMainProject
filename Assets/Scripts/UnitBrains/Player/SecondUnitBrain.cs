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
            ///////////////////////////////////////
            if (!_overheated)
            {
                IncreaseTemperature();
                int t = GetTemperature();

                for (int i = 0; i < t; i++)
                {
                    var projectile = CreateProjectile(forTarget);
                    AddProjectileToList(projectile, intoList);
                }
            }
            

        }

        public override Vector2Int GetNextStep()
        {
            return base.GetNextStep();
        }

        protected override List<Vector2Int> SelectTargets()
        {
            ///////////////////////////////////////
            // Данный вариант решения мне не нравится из-за сохранения цикла while, однако он был в условии.
            // в своих проектах стараюсь его избегать т.к. легко пропустить ошибку и долго восстанавливать прогресс
            ///////////////////////////////////////
            List<Vector2Int> result = GetReachableTargets();
            float nearestEnemy = float.MaxValue;            
            while (result.Count > 1)
            {
                Vector2Int enemyNearest = result[0]; // если мы зашли сюда - ближайшим врагом я могу задать любого противника
                foreach (var target in result)
                {
                    float enemyDistance = DistanceToOwnBase(target);
                    if (nearestEnemy > enemyDistance)
                    {
                        nearestEnemy = enemyDistance;
                        enemyNearest = target;
                    }
             
                }
                result.Clear();
                result.Add(enemyNearest);
            }
            return result;
            ///////////////////////////////////////
        }
        //////////////////////////////////////////
        // иное решение без данного цикла
        //protected override List<Vector2Int> SelectTargets()
        //{
        //    List<Vector2Int> result = GetReachableTargets();
        //    if (result.Count == 0)
        //    {
        //        return result; // заканчиваю с методом если список пуст
        //    }
        //    Vector2Int nearestEnemy = result[0]; // после проверки на пустоту - здесь всегда будет минимум 1 элемент
        //    float nearestEnemyDistance = DistanceToOwnBase(nearestEnemy);
        //    foreach (var target in result)
        //    {
        //        float enemyDistance = DistanceToOwnBase(target);
        //        if (enemyDistance < nearestEnemyDistance)
        //        {
        //            nearestEnemy = target;
        //            nearestEnemyDistance = enemyDistance; 
        //        }
        //    }
        //    return new List<Vector2Int> { nearestEnemy }; // возвращаю новый список а не провожу изменения в старом
        //}
        ///////////////////////////////////////

        public override void Update(float deltaTime, float time)
        {
            if (_overheated)
            {
                Debug.Log("Я перегрет");      
                _cooldownTime += Time.deltaTime;
                float t = _cooldownTime / (OverheatCooldown/10);
                _temperature = Mathf.Lerp(OverheatTemperature, 0, t);
                if (t >= 1)
                {
                    _cooldownTime = 0;
                    _overheated = false;
                    Debug.Log("Я остыл");
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