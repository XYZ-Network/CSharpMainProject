using System.Collections.Generic;
using System.Linq;
using Model;
using Model.Runtime.Projectiles;
using UnitBrains.Pathfinding;
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
        private static int _unitCounter = 0;
        public int UnitNumber { get; private set; }
        public const int MaxTargets = 3;

        private Vector2Int? _selectTarget = null;

        public SecondUnitBrain()
        {
            UnitNumber = _unitCounter++;
        }
        
        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            
            float overheatTemperature = OverheatTemperature;
            float temperature = GetTemperature();

            if (temperature >= overheatTemperature)
            {
                return;
            }

            
            for (int i = 0; i <= temperature; i++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }
            IncreaseTemperature();

            ///////////////////////////////////////
            // Homework 1.3 (1st block, 3rd module)
            ///////////////////////////////////////           

            ///////////////////////////////////////
        }

        public override Vector2Int GetNextStep()
        {
            if (_selectTarget == null || IsTargetInRange(_selectTarget.Value))
                return unit.Pos;

            return unit.Pos.CalcNextStepTowards(_selectTarget.Value);
        }

        protected override List<Vector2Int> SelectTargets()
        {
            _selectTarget = null;
            var result = new List<Vector2Int>();
            var allTargets = GetAllTargets().ToList();

            List<Vector2Int> selectedCandidates = new List<Vector2Int>();

            if (allTargets.Any())
            {
                selectedCandidates = new List<Vector2Int>(allTargets);
                SortByDistanceToOwnBase(selectedCandidates);

                if (selectedCandidates.Count > MaxTargets)
                {
                    selectedCandidates = selectedCandidates.GetRange(0, MaxTargets);
                }
            }
            else
            {
                var enemyBaseId = IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId;
                selectedCandidates.Add(runtimeModel.RoMap.Bases[enemyBaseId]);
            }

            if (selectedCandidates.Any())
            {
                int targetIndex = UnitNumber % selectedCandidates.Count;
                Vector2Int chosenTarget = selectedCandidates[targetIndex];

                if (IsTargetInRange(chosenTarget))
                {
                    result.Add(chosenTarget);
                }
                else
                {
                    _selectTarget = chosenTarget;
                }
            }
            return result;
        }

        public override void Update(float deltaTime, float time)
        {
            if (_overheated)
            {              
                _cooldownTime += deltaTime;
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