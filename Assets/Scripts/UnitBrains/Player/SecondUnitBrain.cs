using System.Collections.Generic;
using System.Linq;
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

        private static int _enemyUnitCounter = 0; //ID
        private int _enemyUnitNumber;
        private const int _maxEnemyInTarget = 3;

        private List<Vector2Int> _enemyOutOfRange = new List<Vector2Int>(); //List for save Dangerous Units.

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            int currentTemeperature = GetTemperature();
            ///////////////////////////////////////
            // Homework 1.3 (1st block, 3rd module)
            ///////////////////////////////////////           
            if (currentTemeperature >= overheatTemperature)
            {
                return;
            }
            else
            {
                for (int i = 0; i <= currentTemeperature; i++)
                {
                    var projectile = CreateProjectile(forTarget);
                    AddProjectileToList(projectile, intoList);
                    //Debug.Log(_temperature + 1);
                }
                IncreaseTemperature();
            }
            ///////////////////////////////////////
        }

        public override Vector2Int GetNextStep()
        {
            Vector2Int target = Vector2Int.zero;
            target = _enemyOutOfRange.Any() ? _enemyOutOfRange[0] : unit.Pos;

            return IsTargetInRange(target) ? unit.Pos : unit.Pos.CalcNextStepTowards(target);
        }

        protected override List<Vector2Int> SelectTargets()
        {
            ///////////////////////////////////////
            // Homework 1.4 (1st block, 4rd module)
            ///////////////////////////////////////
            List<Vector2Int> result = new List<Vector2Int>(GetAllTargets());
            
            Vector2Int targetPosition;

            _enemyOutOfRange.Clear();

            foreach (Vector2Int target in GetAllTargets())
            {
                _enemyOutOfRange.Add(target);
            }

            if (_enemyOutOfRange.Count == 0)
            {
                result.RemoveAt(result.Count - 1);
                int enemyBaseId = IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId;
                Vector2Int enemyBase = runtimeModel.RoMap.Bases[enemyBaseId];
                _enemyOutOfRange.Add(enemyBase);
            }
            else
            {
                SortByDistanceToOwnBase(_enemyOutOfRange);

                int targetIndex = _enemyUnitNumber % _maxEnemyInTarget;

                if (targetIndex > (_enemyOutOfRange.Count - 1))
                {
                    targetPosition = _enemyOutOfRange[0];
                }
                else
                {
                    if (targetIndex == 0)
                    {
                        targetPosition = _enemyOutOfRange[targetIndex];
                    }
                    else
                    {
                        targetPosition = _enemyOutOfRange[targetIndex - 1];
                    }

                }

                if (IsTargetInRange(targetPosition))
                    result.Add(targetPosition);
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