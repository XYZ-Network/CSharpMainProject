using Model;
using Model.Runtime;
using Model.Runtime.Projectiles;
using System.Collections.Generic;
using System.Linq;
using UnitBrains.Pathfinding;
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

        private List<Vector2Int> _targetsToChase = new List<Vector2Int>();
        private static int _unitCounter = 0;
        private readonly int _unitID;
        private const int MaxTargets = 3;


        public SecondUnitBrain()
        {
            _unitID = _unitCounter++;
        }

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            ///////////////////////////////////////
            // Homework 1.3 (1st block, 3rd module)
            ///////////////////////////////////////           
            if (GetTemperature() >= overheatTemperature)
            {
                return;
            }

            int shotsCount = GetTemperature() + 1;

            for (int i = 0; i < shotsCount; i++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }

            IncreaseTemperature();
            ///////////////////////////////////////
        }

        public override Vector2Int GetNextStep()
        {
            if (_targetsToChase.Count > 0)
            {
                return unit.Pos.CalcNextStepTowards(_targetsToChase[0]);
            }
            else
            {
                return unit.Pos;
            }
        }

        protected override List<Vector2Int> SelectTargets()
        {
            ///////////////////////////////////////
            // Homework 5
            ///////////////////////////////////////
            List<Vector2Int> result = new List<Vector2Int>();
            var allTargets = GetAllTargets().ToList();
            var enemyBase = runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId];

            result.Clear();
            _targetsToChase.Clear();


            if (allTargets.Count == 0)
            {
                if (IsTargetInRange(enemyBase))
                    result.Add(enemyBase);
                else
                    _targetsToChase.Add(enemyBase);
                return result;
            }

            SortByDistanceToOwnBase(allTargets);

            int enemyIndex = _unitID % Mathf.Min(allTargets.Count, MaxTargets);
            Vector2Int assignedTarget = allTargets[enemyIndex];

            if (IsTargetInRange(assignedTarget))
                result.Add(assignedTarget);
            else
                _targetsToChase.Add(assignedTarget);

            return result;
            ///////////////////////////////////////
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