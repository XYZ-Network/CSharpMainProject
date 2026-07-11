using Model.Runtime.Projectiles;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        private List<Vector2Int> _unreachableTargets = new();
        private static int _unitCounter = 0;
        private int _unitNumber;
        private const int MaxTargetsCount = 3;

        public SecondUnitBrain()
        {
            _unitNumber = _unitCounter++;
        }

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            ///////////////////////////////////////
            // Homework 1.3 (1st block, 3rd module)
            ///////////////////////////////////////           
            var projectile = CreateProjectile(forTarget);
            
            if (_overheated) return;
            int shotCount = GetTemperature() +1;
            //Debug.Log("Temperature: " + GetTemperature());
            for (int i = 0; i < shotCount; i++)
            {
                AddProjectileToList(projectile, intoList);
            }
            // Debug.Log("Projectile count: " + intoList.Count);
            IncreaseTemperature();
            ///////////////////////////////////////
        }

        public override Vector2Int GetNextStep()
        {
            if (HasTargetsInRange())
                return unit.Pos;

            if (_unreachableTargets.Count > 0)
                return unit.Pos.CalcNextStepTowards(_unreachableTargets[0]);

            return base.GetNextStep();
        }

        protected override List<Vector2Int> SelectTargets()
        {
            ///////////////////////////////////////
            // Homework 1.4 (1st block, 4rd module)
            ///////////////////////////////////////
            List<Vector2Int> sortedTargets = GetAllTargets().ToList();
            List<Vector2Int> targetsInRange = new();
            _unreachableTargets.Clear();
            SortByDistanceToOwnBase(sortedTargets);

            int targetsCount = Mathf.Min(sortedTargets.Count, MaxTargetsCount);
            int targetIndex = _unitNumber % targetsCount;
            Vector2Int selectedTarget = sortedTargets[targetIndex];

            if (IsTargetInRange(selectedTarget))
            {
                targetsInRange.Add(selectedTarget);
            }
            else
            { 
                _unreachableTargets.Add(selectedTarget);
            }
            
            return targetsInRange;
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