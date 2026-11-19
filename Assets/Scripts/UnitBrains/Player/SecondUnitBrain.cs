using System.Collections.Generic;
using Model;
using Model.Runtime.Projectiles;
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

        private List<Vector2Int> _outOfRangeTargets = new List<Vector2Int>(); // цели, к которым нужно идти

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;

            ///////////////////////////////////////
            // Homework 1.3 (1st block, 3rd module)
            ///////////////////////////////////////
            if (GetTemperature() >= OverheatTemperature)
                return;

            IncreaseTemperature();

            for (int i = 0; i < GetTemperature(); i++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
                IncreaseTemperature();

            }
            

            ///////////////////////////////////////
        }



        public override Vector2Int GetNextStep()
        {
            // целей нет - стоим на месте
            if (_outOfRangeTargets.Count == 0)
            {
                return unit.Pos;
            }

            Vector2Int target = _outOfRangeTargets[0]; // цель, к которой идём

            // цель уже в зоне атаки тоже стоим и стреляем
            if (IsTargetInRange(target))
            {
                return unit.Pos;
            }

            // цель далеко делаем шаг к ней
            return unit.Pos.CalcNextStepTowards(target);
        }

        protected override List<Vector2Int> SelectTargets()
        {
            List<Vector2Int> result = new List<Vector2Int>();
            _outOfRangeTargets.Clear(); // очищаем с прошлого раза

            float minDistance = float.MaxValue;
            Vector2Int closestTarget = Vector2Int.zero;
            bool hasTargets = false;

            foreach (Vector2Int target in GetAllTargets()) // все цели, не только достижимые
            {
                hasTargets = true;
                float distance = DistanceToOwnBase(target);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestTarget = target;
                }
            }

            if (hasTargets)
            {
                _outOfRangeTargets.Add(closestTarget); // самая опасная цель - идём к ней

                if (IsTargetInRange(closestTarget)) // если она в зоне досягаемости, ещё и стреляем
                {
                    result.Add(closestTarget);
                }
            }
            else
            {
                // целей нет - идём к базе противника
                int enemyId = IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId;
                _outOfRangeTargets.Add(runtimeModel.RoMap.Bases[enemyId]);
            }

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