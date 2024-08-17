using System.Collections.Generic;
using System.Threading;
using Model.Runtime.Projectiles;
using PlasticGui.WorkspaceWindow;
using PlasticGui.WorkspaceWindow.PendingChanges.Changelists;
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
            // Homework 1.3 (1st block, 3rd module)
            /////////////////////////////////////// 
            if (GetTemperature() >= overheatTemperature)
            {
                return;

            }

            IncreaseTemperature();

            for (int i = 0; i < _temperature; i++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);

            }
            ///////////////////////////////////////
            
        }

        public override Vector2Int GetNextStep()
        {
            return base.GetNextStep();
        }
        /////////////////////////////////////////////////////////////////////////
        protected override List<Vector2Int> SelectTargets()
        {
            ///////////////////////////////////////
            // Homework 1.4 (1st block, 4rd module)
            ///////////////////////////////////////

            //Функция начинается с вызова GetReachableTargets() функции для получения списка достижимых целей.

            List<Vector2Int> result = GetReachableTargets();

            // Иннициализируем новый список, вызываемый tTarget для хранения выбранных обьектов.
             
            List<Vector2Int> tTarget = new List <Vector2Int>();

            //Устанавливаю минимальное расстояние MinDist. на максимально возможное значение 

            float minDist = float.MaxValue;

            //если список достижимых целей пуст,возвращает пустой список

            if (result.Count == 0)
            {
                
                return new List<Vector2Int>();

            }
            //  Выполняем итерацию по списку и вычисляем расстояние между каждой целью и собственной базой,вызывая функцию DistanceToOwnBase().

               foreach (var target in result)

            {

                float  dis = DistanceToOwnBase(target);

                // Если текущее расстояние меньше минимального, минимальное расстояние обновляется, и текущая цель добавляется в tTarget список.
                if (dis < minDist) minDist = dis;

                tTarget.Add(target);

            }   
            // Очищаем исходный result список
            result.Clear(); 

            // Target t список копируется в result список
            result.AddRange(tTarget);

            //Если result список содержит более одного элемента, функция удаляет последний элемент до тех пор, пока не останется только один.
            while (result.Count > 1)
            {

                result.RemoveAt(result.Count - 1);
            }

            //функция возвращает result список, содержащий выбранные цели.
            return result;
        }
        /////////////////////////////////////////////////////////////////////
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