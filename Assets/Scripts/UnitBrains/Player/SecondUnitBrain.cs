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
        private static int counter = 0;
        private int unitNumber;
        private const int constant = 4;
      



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
            float min = float.MaxValue; //до foreach min = миллиард, а после min = 5;

            List<Vector2Int> result = new List<Vector2Int>();


                foreach (Vector2Int target in GetAllTargets())
                {

                    float DistanceToBase = DistanceToOwnBase(target);

                    if (DistanceToBase < min)
                    {
                        min = DistanceToBase;
                        minTarget = target;

                    }

                }

                outOfRangeTargets.Clear();


            if (min < float.MaxValue)
                {
                    if (IsTargetInRange(minTarget))
                    {
                        result.Add(minTarget);
                    }
                    outOfRangeTargets.Add(minTarget);
                }

            
            else //добавляем в цели базу противника если целей нету в списке всех целей
            {
                Vector2Int enemyBase = runtimeModel.RoMap.Bases[
                IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId];
                outOfRangeTargets.Add(enemyBase);
            }

            outOfRangeTargets.Clear();

            foreach (Vector2Int target in GetAllTargets())
            {
                outOfRangeTargets.Add(target);
            }

            if (outOfRangeTargets.Count == 0)
            {
                Vector2Int enemyBase = runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId];
                outOfRangeTargets.Add(enemyBase);
            }

            SortByDistanceToOwnBase(outOfRangeTargets);


            Vector2Int targetPosition;
            if (outOfRangeTargets.Count > 0)
            {
                int targetIndex = counter % outOfRangeTargets.Count;
                targetPosition = outOfRangeTargets[targetIndex];
            }
            else
            {

                targetPosition = runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId];
            }


            if (IsTargetInRange(targetPosition))
            {
                result.Add(targetPosition);
            }

            return result;

        }

        public override Vector2Int GetNextStep()
        {
            Vector2Int targetPos = outOfRangeTargets.Count > 0 ? outOfRangeTargets[0] : unit.Pos;

            if (IsTargetInRange(targetPos))
            {
                return unit.Pos;
            }
            else
            {
                return CalcNextStepTowards(targetPos);
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