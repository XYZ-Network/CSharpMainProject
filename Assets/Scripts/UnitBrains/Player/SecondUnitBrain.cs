using System.Collections.Generic;
using Model;
using Model.Runtime.Projectiles;
using Unity.VisualScripting;
using UnityEngine;
using UnitBrains;
using UnitBrains.Pathfinding;
using System.IO;
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

        private List<Vector2Int> TargetsOutOfRange = new List<Vector2Int>();

        private static int _unitIndex = 0;
        private const int MaxTargets = 4;
        public int UnitID { get; private set; }

        public SecondUnitBrain()
        {
            UnitID = _unitIndex++;
        }

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            float temp = GetTemperature();

            if (temp >= overheatTemperature) return;

            for (int i = 0; i <= temp; i++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }

            IncreaseTemperature();

        }

        public override Vector2Int GetNextStep()
        {
            Vector2Int target = TargetsOutOfRange.Count > 0 ? TargetsOutOfRange[0] : unit.Pos;

            if (IsTargetInRange(target))
            {
                return unit.Pos;
            }
            else
            {
                return unit.Pos.CalcNextStepTowards(target);
            }

        }

        protected override List<Vector2Int> SelectTargets()
        {
            List<Vector2Int> result = new List<Vector2Int>();
            Vector2Int targetPosition;

            TargetsOutOfRange.Clear();

            foreach (Vector2Int target in GetAllTargets())
            {
                TargetsOutOfRange.Add(target);
            }

            if (TargetsOutOfRange.Count == 0)
            {
                int enemyBaseId = IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId;
                Vector2Int enemyBase = runtimeModel.RoMap.Bases[enemyBaseId];
                TargetsOutOfRange.Add(enemyBase);
            }
            else
            {
                SortByDistanceToOwnBase(TargetsOutOfRange);

                int targetIndex = UnitID % MaxTargets;

                if ((targetIndex > (TargetsOutOfRange.Count - 1)) || targetIndex == 0)
                {
                    targetPosition = TargetsOutOfRange[0];
                }
                else
                {
                    targetPosition = TargetsOutOfRange[targetIndex - 1];
                }             

                if (IsTargetInRange(targetPosition))
                    result.Add(targetPosition);
            }
           
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