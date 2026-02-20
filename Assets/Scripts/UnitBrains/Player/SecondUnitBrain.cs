using System.Collections.Generic;
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

        private static int _unitCounter = 0;
        private int _unitNumber;
        private const int MaxSmartTargets = 3;

        protected override void Awake()
        {
            base.Awake();
            _unitNumber = _unitCounter;
            _unitCounter++;
        }
        
        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;

            if (GetTemperature() >= overheatTemperature)
                return;

            IncreaseTemperature();
            int currentTemp = (int)GetTemperature();

            if (currentTemp < 1)
                currentTemp = 1;

            for (int i = 0; i < currentTemp; i++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }
        }

        public override Vector2Int GetNextStep()
        {
            return base.GetNextStep();
        }

        protected override List<Vector2Int> SelectTargets()
        {
            List<Vector2Int> result = new List<Vector2Int>();
            List<Vector2Int> allTargets = GetReachableTargets();

            if (allTargets.Count == 0)
            {
                result.Add(GetEnemyBasePosition());
                return result;
            }

            SortByDistanceToOwnBase(allTargets);

            int smartIndex = _unitNumber % MaxSmartTargets;

            if (smartIndex >= allTargets.Count)
                smartIndex = 0;

            Vector2Int selectedTarget = allTargets[smartIndex];

            if (IsTargetInAttackRange(selectedTarget))
                result.Add(selectedTarget);

            return result;
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
            if (_overheated)
                return (int)OverheatTemperature;
            else
                return (int)_temperature;
        }

        private void IncreaseTemperature()
        {
            _temperature += 1f;

            if (_temperature >= OverheatTemperature)
                _overheated = true;
        }
    }
}