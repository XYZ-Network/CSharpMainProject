using System.Collections.Generic;
using System.Linq;
using Model;
using Model.Runtime.Projectiles;
using UnityEngine;
using Utilities;

namespace UnitBrains.Player
{
    public class SecondUnitBrain : DefaultPlayerUnitBrain
    {
        public static int UnitCounter = 0;
        public override string TargetUnitName => "Cobra Commando";
        private const float OverheatTemperature = 3f;
        private const float OverheatCooldown = 2f;
        private float _temperature = 0f;
        private float _cooldownTime = 0f;
        private bool _overheated;
        private int _unitNumber;
        private int _maxTargetCounter = 3;
        private List<Vector2Int> _targets = new();
        
        public SecondUnitBrain()
        {
            _unitNumber = ++UnitCounter;
        }
        
        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            ///////////////////////////////////////
            // Homework 1.3 (1st block, 3rd module)
            ///////////////////////////////////////           
            var projectile = CreateProjectile(forTarget);
            AddProjectileToList(projectile, intoList);
            ///////////////////////////////////////
        }

        public override Vector2Int GetNextStep()
        {
            if (_targets.Count > 0)
            {
                return unit.Pos.CalcNextStepTowards(_targets[_unitNumber % _targets.Count]);
            }

            return unit.Pos;
        }

        protected override List<Vector2Int> SelectTargets()
        {
            var result = new List<Vector2Int>();
            var allTargets = GetAllTargets().ToList();

            _targets.Clear();

            if (allTargets.Count == 0)
            {
                var enemyBase = runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.PlayerId : RuntimeModel.BotPlayerId];
                allTargets.Add(enemyBase);
                return _targets;
            }

            SortByDistanceToOwnBase(allTargets);

            for (var i = 0; i < Mathf.Min(_maxTargetCounter, allTargets.Count); i++)
            {
                if (IsTargetInRange(allTargets[i]))
                {
                    result.Add(allTargets[i]);
                }
                else
                {
                    _targets.Add(allTargets[i]);
                }
            }

            if (result.Count > 0)
            {
                result.RemoveAt(result.Count - 1);
                _targets.AddRange(result);
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