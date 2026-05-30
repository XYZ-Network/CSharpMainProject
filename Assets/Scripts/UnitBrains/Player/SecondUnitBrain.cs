using Model;
using Model.Runtime.Projectiles;
using System.Collections.Generic;
using UnitBrains.Buffs;
using UnityEngine;

namespace UnitBrains.Player
{
    public class SecondUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Cobra Commando";
        private const float OverheatTemperature = 3f;
        private const float OverheatCooldown = 2f;
        private const int MaxTargets = 3;
        private static int _unitCounter = 0;
        private readonly int _unitNumber;
        private float _temperature = 0f;
        private float _cooldownTime = 0f;
        private bool _overheated;

        private bool _hasDoubleShot = false;

        public SecondUnitBrain()
        {
            _unitNumber = _unitCounter++;
        }

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float temperature = GetTemperature();
            if (temperature >= OverheatTemperature)
                return;

            // Двойной выстрел если есть бафф
            int shotsCount = _hasDoubleShot ? 2 : 1;

            for (int i = 0; i <= temperature; i++)
            {
                for (int s = 0; s < shotsCount; s++)
                {
                    var projectile = CreateProjectile(forTarget);
                    AddProjectileToList(projectile, intoList);
                }
            }

            IncreaseTemperature();
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
                Vector2Int baseTarget = runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId];
                if (IsTargetInRange(baseTarget))
                    result.Add(baseTarget);
                return result;
            }

            allTargets.Sort((a, b) => DistanceToOwnBase(a).CompareTo(DistanceToOwnBase(b)));

            int targetIndex = _unitNumber % Mathf.Min(MaxTargets, allTargets.Count);
            Vector2Int selectedTarget = allTargets[targetIndex];

            if (IsTargetInRange(selectedTarget))
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

            // Проверяем бафф двойного выстрела
            if (unit != null)
            {
                _hasDoubleShot = BuffSystem.HasBuff(unit, "Double Shot");
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

        // Публичный метод для наложения баффа (вызывается извне)
        public void ApplyDoubleShotBuff(float duration = 5f)
        {
            if (unit != null)
            {
                BuffSystem.ApplyBuff(unit, new DoubleShotBuff(duration));
            }
        }
    }
}