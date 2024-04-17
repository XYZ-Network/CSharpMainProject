using Model;
using Model.Runtime.Projectiles;
using System.Collections.Generic;
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
        private List<Vector2Int> DangerTargetsOutOfRange = new List<Vector2Int>();

        // HomeWork Seven
        private static int _unitIdCounter = 0;
        public int UnitId { get; private set; }
        private const int MaxTargets = 3;

        public SecondUnitBrain()
        {
            UnitId = _unitIdCounter++;
        }

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            ///////////////////////////////////////
            // Homework 1.3 (1st block, 3rd module)
            int currentTemperature = GetTemperature();

            if (currentTemperature >= overheatTemperature)
            {
                return;
            }

            ///////////////////////////////////////

            for (int i = 0; i < currentTemperature + 1; i++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }

            IncreaseTemperature();
            ///////////////////////////////////////
        }

        public override Vector2Int GetNextStep()
        {
            if (DangerTargetsOutOfRange.Count > 0)
            {
                Vector2Int currentTarget = DangerTargetsOutOfRange[0];

                if (IsTargetInRange(currentTarget))
                {
                    return unit.Pos;
                }
                else
                {
                    return unit.Pos.CalcNextStepTowards(currentTarget);
                }
            }
            else
            {
                return unit.Pos;
            }
        }

        protected override List<Vector2Int> SelectTargets()
        {
            List<Vector2Int> result = new List<Vector2Int>();

            DangerTargetsOutOfRange.Clear();
            DangerTargetsOutOfRange.AddRange(GetAllTargets());

            if (DangerTargetsOutOfRange.Count > 0)
            {
                SortByDistanceToOwnBase(DangerTargetsOutOfRange);

                Vector2Int targetForAttack = GetTargetForAttackById();

                if (IsTargetInRange(targetForAttack))
                {
                    result.Add(targetForAttack);
                }
            }
            else
            {
                Vector2Int enemyBase = runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId];
                DangerTargetsOutOfRange.Add(enemyBase);
            }

            return result;
            ///////////////////////////////////////
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

        private Vector2Int GetTargetForAttackById()
        {
            int targetIndex = UnitId % MaxTargets;

            if (targetIndex > (DangerTargetsOutOfRange.Count - 1) || targetIndex == 0)
            {
                return DangerTargetsOutOfRange[0];
            }
            else
            {
                return DangerTargetsOutOfRange[targetIndex - 1];
            }
        }
    }
}