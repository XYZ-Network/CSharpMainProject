using Codice.Client.Common;
using GluonGui.Dialog;
using Model;
using Model.Runtime;
using Model.Runtime.Projectiles;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
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
        private  List<Vector2Int> _priorityTarget = new();
        private static int _count = 0;
        private int UnitId;
        private const int MaxTarget = 3;
        private bool _isDoubleShotEnabled = false;

        public void EnableDoubleShot()
        {
            _isDoubleShotEnabled = true;    
        }
        public void DisableDoubleShot()
        {
            _isDoubleShotEnabled = false;
           
        }

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            if (_isDoubleShotEnabled) 
            {
                var projectile1 = CreateProjectile(forTarget);
                var projectile2 = CreateProjectile(forTarget);
                AddProjectileToList(projectile1, intoList);
                AddProjectileToList(projectile2, intoList);
                return; 
            }
            float overheatTemperature = OverheatTemperature;
            if (GetTemperature() < overheatTemperature)
            {
                IncreaseTemperature();
            }
            else
            {
                return;
            }

            for (int i = 0; i < GetTemperature(); i++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }
        }

        public override Vector2Int GetNextStep()
        {
            base.GetNextStep();
            var target = _priorityTarget.Count > 0 ? _priorityTarget[0] : unit.Pos;
            
            return IsTargetInRange(target) ? unit.Pos : unit.Pos.CalcNextStepTowards(target);
        }

        protected override List<Vector2Int> SelectTargets()
        {

            if (UnitId == 0)
            {
                UnitId = _count;
                _count++;
                Debug.Log($"создан {UnitId} с {_count}");
            }
            _priorityTarget.Clear();
            var result = new List<Vector2Int>();
            var allTargets = GetAllTargets().ToList();

            if (allTargets.Count() > 0)
            {
                SortByDistanceToOwnBase(allTargets);

                var priorityTargets = allTargets.Take(MaxTarget).ToList();
                int targetIndex = UnitId % priorityTargets.Count();
                var bestTarget = priorityTargets[targetIndex];

                _priorityTarget.Clear();
                _priorityTarget.Add(bestTarget);
                if (IsTargetInRange(bestTarget))
                {
                    result.Add(bestTarget);
                }

            }
            else
            {

                _priorityTarget.Add(runtimeModel.RoMap.Bases[
                    IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId]);
            }

            return result;
        }






        public override void Update(float deltaTime, float time)
        {
            if (_overheated)
            {
                _cooldownTime += UnityEngine.Time.deltaTime;
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