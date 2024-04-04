using System.Collections.Generic;
using GluonGui.Dialog;
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
            ///////////////////////////////////////////
            ///// Homework 1.3 (1st block, 3rd module)
            float overheatTemperature = OverheatTemperature;
            float temp = GetTemperature();
            if (temp >= overheatTemperature)
            {
                return;
            }
            IncreaseTemperature();           
            for (float i = 0; i <= temp; i++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }
        }        
        ///////////////////////////////////////////////////
       
       
        public override Vector2Int GetNextStep()
        {
            return base.GetNextStep();
        }

        protected override List<Vector2Int> SelectTargets()
        {
            ///////////////////////////////////////
            // Homework 1.4 (1st block, 4rd module)
            
            List<Vector2Int> result = GetReachableTargets(); //объявили список, содержащий тип данных "координаты" и присвоили ему значение = метод GetReachableTargets
            Vector2Int nearEnemy = Vector2Int.zero; //ввели переменную nearEnemy и присвоили ей значение = нулевым координатам вектора Vector2Int
            float minDistance = float.MaxValue; ///ввели новую переменную minDistance и в качестве критерия перебора использовали MaxValue
            foreach (var target in result) //производим действия с каждым элементом target внутри списка result 
            {
                float distance = DistanceToOwnBase(target); //вводим переменную "distance", равную методу(расстояние от цели до своей базы), ориентированному на элемент target
                if (distance < minDistance) minDistance = distance; // перебираем все значения distance, пока не найдем самое большое и приравниваем значение к нему                   
                {                    
                    nearEnemy = target; //в случае, если найдено минимальное расстояние, назначаем ближайшего врага целью для атаки                
                }
                //Debug.Log("расстояние от базы до ближайшей цели = " + DistanceToOwnBase(target)); //используем сообщение для проверки расстояния от базы до ближайшей цели
            } 
            while (result.Count > 1) //пока кол-во элементов в списке > 1
            {
                result.RemoveAt(result.Count - 1);  //выполняется очистка списка 
            }
            // в случае, если ближайшая цель найдена:
            if (minDistance != float.MaxValue)
            {
                result.Clear(); //очищаем список result
                result.Add(nearEnemy); //добавляем в него ближайшую цель
            }                       
            return result; ///затем возвращаем список result

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