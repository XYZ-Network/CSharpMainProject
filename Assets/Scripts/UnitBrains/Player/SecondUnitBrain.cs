using System;
using System.Collections.Generic;
using System.Linq;
using Codice.CM.Triggers;
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
        private List<Vector2Int> UnreachableTargets = new List<Vector2Int>();
        private float _temperature = 0f;
        private float _cooldownTime = 0f;
        private bool _overheated;
        
        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            ///////////////////////////////////////
            // Homework 1.3 (1st block, 3rd module)
            ///////////////////////////////////////
            
            int currentTemperature = GetTemperature();
            if (currentTemperature >= overheatTemperature)
            {
                return;
            }

            var projectile = CreateProjectile(forTarget);
            for(float projectileCount = 0; projectileCount <= currentTemperature; projectileCount++)
            {   
                AddProjectileToList(projectile, intoList);
            }
            IncreaseTemperature();
        }

        public override Vector2Int GetNextStep()
        {
            Vector2Int target = Vector2Int.zero;
            target = UnreachableTargets.Any() ? UnreachableTargets[0] : unit.Pos;

            return IsTargetInRange(target) ? unit.Pos : unit.Pos.CalcNextStepTowards(target);
        }

        protected override List<Vector2Int> SelectTargets()
        {
            ///////////////////////////////////////
            // Homework 1.4 (1st block, 4rd module)
            ///////////////////////////////////////
            
            UnreachableTargets.Clear();
            
            List<Vector2Int> result = new List<Vector2Int>();            
            TargetInfo mainTarget = GetNearestTarget(GetAllTargets().ToList());

            if (mainTarget.Distance < float.MaxValue) 
            {
                UnreachableTargets.Add(mainTarget.Target);

                if (IsTargetInRange(mainTarget.Target))
                {
                    result.Add(mainTarget.Target);
                }

            }
            else
            {
                result.Add(GetEnemyBase());
            }
            
            return result;
        }

        private TargetInfo GetNearestTarget(List<Vector2Int> targets)
        {
            float minDistanceToOwnBase = float.MaxValue;
            Vector2Int mainTarget = Vector2Int.zero;

            foreach (Vector2Int target in targets)
            {
                float targetDistanceToOwnBase = DistanceToOwnBase(target);

                if (targetDistanceToOwnBase < minDistanceToOwnBase)
                {
                    minDistanceToOwnBase = targetDistanceToOwnBase;
                    mainTarget = target;
                }

            }

            TargetInfo targetInfo = new TargetInfo(mainTarget, minDistanceToOwnBase);

            return targetInfo;
        }

        private Vector2Int GetEnemyBase()
        {
            return runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId];
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

        [Serializable]
        public class TargetInfo
        {
            private Vector2Int _target;
            private float _distanceToTarget;

            public Vector2Int Target => _target;
            public float Distance => _distanceToTarget;

            public TargetInfo(Vector2Int target, float distanceTotarget)
            {
                _target = target;
                _distanceToTarget = distanceTotarget;
            }
        }
    }
}