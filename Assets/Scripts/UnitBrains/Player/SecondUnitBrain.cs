using System.Collections.Generic;
using System.Linq;
using GluonGui.Dialog;
using Model;
using Model.Runtime.Projectiles;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Utilities;

namespace UnitBrains.Player
{
    public class SecondUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Cobra Commando";
        private const float OverheatTemperature = 3f;
        private List<Vector2Int> targetsOutRangeUnit = new();
        private const float OverheatCooldown = 2f;
        private float _temperature = 0f;
        private float _cooldownTime = 0f;
        private bool _overheated;

        public static int CounterUnit { get; private set; } = 0;
        public int ID { get; set; } = CounterUnit++;
        public const int MaxUnitTargetToAttack = 3;

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;

            if (GetTemperature() >= overheatTemperature)
                return;

            IncreaseTemperature();

            for (int i = 0; i < GetTemperature(); i++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }
        }

        public override Vector2Int GetNextStep()
        {
            if (targetsOutRangeUnit.Count > 0)
                return unit.Pos.CalcNextStepTowards(targetsOutRangeUnit[0]);

            return unit.Pos;
        }

        protected override List<Vector2Int> SelectTargets()
        {
            List<Vector2Int> result = new List<Vector2Int>();
            Vector2Int targetPosition;

            targetsOutRangeUnit.Clear();

            foreach (Vector2Int target in GetAllTargets())
            {
                targetsOutRangeUnit.Add(target);
            }

            if (targetsOutRangeUnit.Count == 0)
            {
                result.RemoveAt(result.Count - 1);
                int enemyBaseId = IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId;
                Vector2Int enemyBase = runtimeModel.RoMap.Bases[enemyBaseId];
                targetsOutRangeUnit.Add(enemyBase);
            }
            else
            {
                SortByDistanceToOwnBase(targetsOutRangeUnit);

                int targetIndex = ID % MaxUnitTargetToAttack;

                if (targetIndex > (targetsOutRangeUnit.Count - 1))
                {
                    targetPosition = targetsOutRangeUnit[0];
                }
                else
                {
                    if (targetIndex == 0)
                    {
                        targetPosition = targetsOutRangeUnit[targetIndex];
                    }
                    else
                    {
                        targetPosition = targetsOutRangeUnit[targetIndex - 1];
                    }

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
