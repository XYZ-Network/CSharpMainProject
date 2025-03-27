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
        private List<Vector2Int> _targetsNotInRange = new List<Vector2Int>();

        private static int _idCounter = 0;
        private int _unitId = _idCounter++;
        private const int nearestTargetsCount = 3;

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            ///////////////////////////////////////
            // Homework 1.3 (1st block, 3rd module)
            ///////////////////////////////////////           
            var projectile = CreateProjectile(forTarget);
            AddProjectileToList(projectile, intoList);
            ///////////////////////////////////////
        }

        //public override Vector2Int GetNextStep()
        //{
        //    Vector2Int position = unit.Pos;

        //    if (_targetsNotInRange.Any())
        //    {
        //        Vector2Int nextPosition = _targetsNotInRange[0];
        //        position = position.CalcNextStepTowards(nextPosition);
        //    }
        //    return position;
        //}

        private int CalcTargetId(int allTargetsCount)
        {
            int targetsCount = nearestTargetsCount < allTargetsCount ? nearestTargetsCount : allTargetsCount;
            
            return _unitId % targetsCount == 0 ? targetsCount : _unitId % targetsCount; ;
        }

        //protected override List<Vector2Int> SelectTargets()
        //{
        //    List<Vector2Int> allTargets = new List<Vector2Int>();

        //    foreach (var target in GetAllTargets())
        //    {
        //        allTargets.Add(target);
        //    }

        //    if (!allTargets.Any())
        //    {
        //        int enemyId = IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId;
        //        Vector2Int enemyBasePos = runtimeModel.RoMap.Bases[enemyId];
        //        allTargets.Add(enemyBasePos);
        //    }

        //    SortByDistanceToOwnBase(allTargets);
        //    Vector2Int targetForAttack = allTargets[CalcTargetId(allTargets.Count) - 1];

        //    List<Vector2Int> result = new List<Vector2Int>();

        //    _targetsNotInRange.Clear();
        //    if (IsTargetInRange(targetForAttack))
        //        result.Add(targetForAttack);
        //    else
        //        _targetsNotInRange.Add(targetForAttack);
        //    return result;
        //}

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