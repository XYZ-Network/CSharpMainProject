using System.Collections.Generic;
using Model.Runtime.Projectiles;
using Unity.VisualScripting;
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

        List<Vector2Int> _notTargetsInRange = new List<Vector2Int>();

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
            Vector2Int bestTarg = Vector2Int.zero;

            foreach (Vector2Int targ in _notTargetsInRange)
            {
                if (IsTargetInRange(targ))
                    bestTarg = targ;
                else
                    bestTarg.CalcNextStepTowards(targ);
                    bestTarg = targ;
            }
            
            return bestTarg;
        }

        protected override List<Vector2Int> SelectTargets()
        {
            List<Vector2Int> result = new List<Vector2Int>();
            
            foreach (Vector2Int target in GetAllTargets())
            {
                result.Add(target);
            }

            float min = float.MaxValue;
            Vector2Int bestTarget = Vector2Int.zero;

            foreach (Vector2Int i in result)
            {
                float distance = DistanceToOwnBase(i);

                if (min > distance)
                    min = distance;
                    bestTarget = i;
            }

            result.Clear();

            if (IsTargetInRange(bestTarget))
                result.Add(bestTarget);
            else
                _notTargetsInRange.Add(bestTarget);

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