using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using Codice.Client.Common.GameUI;
using JetBrains.Annotations;
using Model;
using Model.Runtime.Projectiles;
using UnityEditor.Experimental.GraphView;
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
        private List<Vector2Int> targetToGo = new();
        private IEnumerable<Vector2Int> resultToGo;
        //private int x = 0;
        private static int counter = 0;
        private int id = 0;
        private int maxTargets = 3;


        public SecondUnitBrain()
            {
                id = counter++;
                //Debug.Log("id = " + id);
            }
        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            ///////////////////////////////////////
            // Homework 1.3 (1st block, 3rd module)
            ///////////////////////////////////////     
            float getTemperature = GetTemperature();
            if (getTemperature < overheatTemperature)
            {
                for (int i = 0; i <= getTemperature; i++)
                {
                    var projectile = CreateProjectile(forTarget);
                    AddProjectileToList(projectile, intoList);
                }
                IncreaseTemperature();
            }
            ///////////////////////////////////////
        }

        public override Vector2Int GetNextStep()
        {

            //если цель в области атаки

            //if (!targetToGo.Any() || IsTargetInRange(targetToGo[0]))// если целей нет или в области атаки
            //{
            //    return this.unit.Pos;
            //}
            //если цель вне области атаки
            //Debug.Log(unit.Pos.CalcNextStepTowards(targetToGo));
            //Debug.Log("GetNextStep" + x);
            //x++;
            //return unit.Pos.CalcNextStepTowards(targetToGo.First());
            return base.GetNextStep();
        }

        protected override List<Vector2Int> SelectTargets()
        {
            ///////////////////////////////////////
            // Homework 1.4 (1st block, 4rd module)
            ///////////////////////////////////////
            //List<Vector2Int> result = GetReachableTargets();
            List<Vector2Int> result = new List<Vector2Int>();
            resultToGo = GetAllTargets();
            Vector2Int bestTarget = Vector2Int.zero;
            Vector2Int adstargetInRange = Vector2Int.zero;
            float dist = float.MaxValue;
            //float distInRange = float.MaxValue;
            //List<Vector2Int> dangerToGo = new List<Vector2Int>();
            //while (result.Count > 1)
            
            if (resultToGo.Any())  // проверили что есть цели в целом=)
            {
                //Debug.Log("зашли в resultToGo.Any()"); 
                result.Clear();
                foreach (var target in resultToGo)  // перевели список целей в резалт
                {
                    if (DistanceToOwnBase(target) < dist)
                    {
                        dist = DistanceToOwnBase(target);
                        bestTarget = target;
                        result.Add(target);
                        //Debug.Log("targetToGo = " + targetToGo);
                    }
                }

                SortByDistanceToOwnBase(result);  // отсортировали список
                
                foreach (var target in result) // для целей добавленных в резалт рассчитываем кого будем атаковать
                {
                    if (this.id > 2) // целей больше или равно 3
                    {
                        

                        int a = this.id;
                        int b = 0;
                        if (result.Count <= maxTargets)  // целей меньше или равно maxTargets (3)
                        {
                            b = result.Count;
                        }
                        else  // целей больше 3, используем лимитер
                        {
                            b = maxTargets;
                        }
                        int index = a % b;  // считаем по остатку
                        bestTarget = result[index]; // цель - резалт по индексу - остаток после деления. 3 будет 0, 4 будет 1, 5 - 2, 6 снова 0 и т.д.

                    }
                    else
                    {
                        
                        if (this.id < result.Count)  // count - количество целей, индекс должен быть -1 к count
                        {
                            //Debug.Log("порушенный id = " + id);
                            //Debug.Log("резалт капасити " + result.Count);
                            bestTarget = result[this.id]; // цель - резалт по индексу. 0=0 1=1 2=2
                        }
                        else 
                        {
                            bestTarget = result[this.id % (result.Count)];  // если целей меньше юнитов
                        }
                    }


                }
            }
            else  // если целей нет, передаем базу
            {
                //Debug.Log("ищем базу"); 
                result.Clear();
                bestTarget = (runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId]);
                result.Add(bestTarget);
                return result;
            }

           
            if (!IsTargetInRange(bestTarget)) // если цель вне зоны поражения, передаем цель в "дойти до"
            {
                targetToGo.Clear();
                targetToGo.Add(bestTarget);
                result.Clear();
                return result;
            }

            result.Clear();
            result.Add(bestTarget);
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