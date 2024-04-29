using System;
using System.Collections.Generic;
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
        private List<Vector2Int> _currentTargets = new List<Vector2Int>();
        private static int _unitCounter = 0;
        private int _unitNumber=_unitCounter++;
        private const int MaxTargets = 3;


        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            float temp = GetTemperature();

            if (temp >= overheatTemperature) return;

            IncreaseTemperature();

            for (int i = 0; i <= temp; i++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);

            }

        }


        public override Vector2Int GetNextStep()
        {
            Vector2Int target = Vector2Int.zero; 
            if (_currentTargets.Count > 0)
                target = _currentTargets[0]; 
            else
                target = unit.Pos; 

            if (IsTargetInRange(target))
                return unit.Pos;
            else
                return target; 
        }

        protected List<Vector2Int> GetAllTargets()
        {
            List<Vector2Int> allTargets = new List<Vector2Int>();

            return allTargets;
        }


        protected override List<Vector2Int> SelectTargets()
        {
            List<Vector2Int> result = new List<Vector2Int>();
            _currentTargets.Clear();
            _currentTargets.AddRange(GetAllTargets());

            if (_currentTargets.Count == 0)
            {
                _currentTargets.Add(runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId]);
            }

            SortByDistanceToOwnBase(_currentTargets);

            Vector2Int targetPosition = DetermineTargetPosition();

            if (IsTargetInRange(targetPosition))
            {
               result.Add(targetPosition);
            }

            return result;
        }

        private void SortTargetsByDistanceToBase()
        {
            _currentTargets.Sort((a, b) =>
            {
                float distanceToA = Vector2Int.Distance(a, unit.Pos);
                float distanceToB = Vector2Int.Distance(b, unit.Pos);
                return distanceToA.CompareTo(distanceToB);
            });
        }

        private Vector2Int DetermineTargetPosition()
        {
            int targetIndex = _unitNumber % MaxTargets;
            int bestTarget = Math.Min(targetIndex, _currentTargets.Count - 1);
            return _currentTargets[targetIndex];
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