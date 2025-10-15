using System;
using System.Collections.Generic;
using System.Linq;
using Model;
using Model.Runtime.Projectiles;
using UnityEngine;
using UnityEngine.UIElements;
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

        List<Vector2Int> possibleTargets = new List<Vector2Int>();
        List<Vector2Int> unReachableTargets = new List<Vector2Int>();
        List<Vector2Int> result = new List<Vector2Int>();

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            ///////////////////////////////////////
            // Homework 1.3 (1st block, 3rd module)
            ///////////////////////////////////////
            ///

            Debug.Log($"Current temperature at start: {_temperature}");

            float currentTemperature = GetTemperature();

            if (currentTemperature >= overheatTemperature) {
                return;
            }
            
            for (int i=0; i <= currentTemperature; i++ ) {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }
            

            IncreaseTemperature();
            ///////////////////////////////////////
        }

        public override Vector2Int GetNextStep()
        {
            Vector2Int position = new Vector2Int();
            Vector2Int targetPosition = new Vector2Int();

            //Если в доступных целях кто-то есть, стоим на месте и атакуем
            if (result.Count > 0) {

                position = unit.Pos;

            //Если в недоступных целях кто-то есть, движемся к первой цели в этом списке
            } else if (unReachableTargets.Count() > 0) {

                targetPosition = unReachableTargets.First();
                position = unit.Pos.CalcNextStepTowards(targetPosition);

            //Если везде пусто, движемся к базе противника
            } else {

                targetPosition = runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId];
                position = unit.Pos.CalcNextStepTowards(targetPosition);
            }

            Debug.Log(position);
            return position;
        }

        protected override List<Vector2Int> SelectTargets() {
            ///////////////////////////////////////
            // Homework 1.4 (1st block, 4rd module)
            ///////////////////////////////////////
            possibleTargets = GetAllTargets().ToList();
            Vector2Int active_target = new Vector2Int();
            float closestDistance = float.MaxValue;

            unReachableTargets.Clear();
            result.Clear();

            if (possibleTargets.Any()) {

                foreach (Vector2Int target in possibleTargets) {

                    float distance = DistanceToOwnBase(target);

                    if (distance < closestDistance) {

                        closestDistance = distance;
                        active_target = target;

                    }
                    //Если ближайшая цель в зоне доступа, добавляем в result, если нет - идем к ней
                    if (GetReachableTargets().Contains(active_target)) {

                        result.Clear();
                        result.Add(active_target);
                    } 
                    
                    else

                        unReachableTargets.Add(active_target);
                }
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