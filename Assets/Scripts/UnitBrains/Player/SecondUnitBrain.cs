using System.Collections.Generic;
using System.Linq;
using Model;
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
        private List<Vector2Int> outOfRangeTargets = new List<Vector2Int>(); //список целей, которые вне досягаемости

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {

            float overheatTemperature = OverheatTemperature;
            float currenttemp = GetTemperature();

            if (currenttemp >= overheatTemperature)
            {
                return;
            }

            IncreaseTemperature();

            for (int i = 0; i <= currenttemp; i++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }

        }


        protected override List<Vector2Int> SelectTargets()
        {

            Vector2Int minTarget = Vector2Int.zero;
            float min = float.MaxValue;

            List<Vector2Int> result = GetAllTargets().ToList();

            if (result.Count > 0) //проверяем, есть ли хотя бы одна цель и если есть - определяем ближайшую
            {
                foreach (Vector2Int target in GetAllTargets())
                {

                    float DistanceToBase = DistanceToOwnBase(target);

                    if (DistanceToBase < min)
                    {
                        minTarget = target;

                        if (IsTargetInRange(target)) //если цель в зоне досягаемости - добавляем в result
                        {
                            result.Add(minTarget);
                        }

                        outOfRangeTargets.Add(target); //если цель вне зоны досягаемости, записываем ее в список outOfRangeTargets
                    }
                }
                

            }
            else //добавляем в цели базу противника если целей нету в списке всех целей
            {
                Vector2Int enemyBase = runtimeModel.RoMap.Bases[
                IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId];
                result.Add(enemyBase);
            }

            return result;

        }

        public override Vector2Int GetNextStep()
        {
            {
                List<Vector2Int> targets = SelectTargets();

                if (targets.Count > 0)
                {
                    foreach (Vector2Int targetPos in targets)
                    {

                        if (IsTargetInRange(targetPos))
                        {
                           
                            return unit.Pos;
                        }
                        else
                        {
                            
                            return CalcNextStepTowards(targetPos); // цель вне зоны досягаемости, вызываем метод для расчета следующего шага
                        }
                    }
                }

               
                return unit.Pos;  // если целей нет, возвращаем текущую позицию юнита
            }


        }

        public override void Update(float deltaTime, float time)
        {
            if (_overheated)
            {
                _cooldownTime += Time.deltaTime;
                float t = _cooldownTime / (OverheatCooldown / 10);
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
            if (_overheated) return (int)OverheatTemperature;
            else return (int)_temperature;
        }

        private void IncreaseTemperature()
        {
            _temperature += 1f;
            if (_temperature >= OverheatTemperature) _overheated = true;
        }
    }
}