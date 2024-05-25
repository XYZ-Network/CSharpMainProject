using System;
using System.Collections.Generic;
using Model;
using Model.Runtime.Projectiles;
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
        private static int _counter = 0;
        private const int _maxTargets = 4;
        private int _unitCount = _counter++;

        List<Vector2Int> _outsideTargets = new List<Vector2Int>();

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
            Vector2Int currentTarget = _outsideTargets.Count > 0 ? _outsideTargets[0] : unit.Pos;

            if (IsTargetInRange(currentTarget))
            {
                return unit.Pos;
            }
            else
            {
                return CalcNextStepTowards(currentTarget);
            }
        }

        protected override List<Vector2Int> SelectTargets()
        {
            List<Vector2Int> allTargets = new List<Vector2Int>();
            List<Vector2Int> targetsForAttack = new List<Vector2Int>();

            Vector2Int closestTarget = new Vector2Int();

            int indexCurrentTargetForAttack = _unitCount % _maxTargets;

            foreach (var target in GetAllTargets())
            {
                allTargets.Add(target);
            }

            SortByDistanceToOwnBase(allTargets);
            _outsideTargets.Clear();
            int minWeight = int.MaxValue;

            for (int i = 0; i < allTargets.Count; i++)
            {
                int weight = Mathf.Abs(i - indexCurrentTargetForAttack);

                if (IsTargetInRange(allTargets[i]))
                {
                    if (weight < minWeight)
                    {
                        minWeight = weight;
                        closestTarget = allTargets[i];
                        targetsForAttack.Add(closestTarget);
                    }
                }
                else
                {
                    _outsideTargets.Add(allTargets[i]);
                }
            }

            if (allTargets.Count == 1)
            {
                var enemyBase = runtimeModel.RoMap.Bases[
                    IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId];
                targetsForAttack.Add(enemyBase);
            }
            return targetsForAttack;
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