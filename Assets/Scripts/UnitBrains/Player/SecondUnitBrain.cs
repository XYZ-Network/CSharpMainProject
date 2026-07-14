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

        private Vector2Int _targetToChase = new Vector2Int(-1, -1);

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
            for (int i = 0; i < _temperature+1; i++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }
            //Debug.Log(intoList.Count);
            IncreaseTemperature();
            ///////////////////////////////////////
        }

        public override Vector2Int GetNextStep()
        {
            Vector2Int currentPos = unit.Pos;

            if (_targetToChase.x == -1 && _targetToChase.y == -1)
            {
                return currentPos;
            }

            if (IsTargetInRange(_targetToChase))
            {
                _targetToChase = new Vector2Int(-1, -1);
                return currentPos;
            }
            
            Vector2Int nestStep = currentPos.CalcNextStepTowards(_targetToChase);
            return nestStep;
        }

        protected override List<Vector2Int> SelectTargets()
        {
            ///////////////////////////////////////
            // Homework 1.4 (1st block, 4rd module)
            ///////////////////////////////////////
            List<Vector2Int> allTargets = GetAllTargets().ToList();

            if (allTargets.Count == 0)
            {
                int enemyBaseId = IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId;
                Vector2Int enemyBasePos = runtimeModel.RoMap.Bases[enemyBaseId];
                allTargets.Add(enemyBasePos);
            }

            Vector2Int choseTarget = allTargets[0];
            float minDistToOwnBase = DistanceToOwnBase(choseTarget);
            foreach (Vector2Int target in allTargets)
            {
                float dist = DistanceToOwnBase(target);
                if (dist < minDistToOwnBase)
                {
                    minDistToOwnBase = dist;
                    choseTarget = target;
                } 
            }

            bool inAttackRange = IsTargetInRange(choseTarget);

            List<Vector2Int> result = new List<Vector2Int>();

            if (inAttackRange)
            {
                result.Add(choseTarget);
                _targetToChase = new Vector2Int(-1, -1);
            }
            else
            {
                _targetToChase = choseTarget;
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