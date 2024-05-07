using System.Collections.Generic;
using Model.Runtime.Projectiles;
using Unity.VisualScripting;
using UnityEngine;
using Model;
using UnityEngine.UIElements;
using Utilities;
using GluonGui.Dialog;
using System.Linq;
using PlasticGui.WorkspaceWindow;

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
        private List<Vector2Int> _notReachebleTarget = new List<Vector2Int>();
        private static int _unitCounter = 0;
        private int _unitNumber = _unitCounter++;
        private const int _maxUnits = 3;

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            if (GetTemperature() >= overheatTemperature)
            {
                return;
            }
            IncreaseTemperature();
            ///////////////////////////////////////
            // Homework 1.3 (1st block, 3rd module)
            ///////////////////////////////////////           
            ///

            for (int i = 0; i < GetTemperature(); i++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }
            ///////////////////////////////////////
        }

        public override Vector2Int GetNextStep()
        {
            Vector2Int target = Vector2Int.zero;
            List<Vector2Int> targetsInRange = SelectTargets();
            if (_notReachebleTarget.Count == 0) 
            {
                target = unit.Pos;
            }
            else target = _notReachebleTarget[0];

            if (IsTargetInRange(target)) 
            { 
                return unit.Pos; 
            }
            else return unit.Pos.CalcNextStepTowards(target);
        }

        protected override List<Vector2Int> SelectTargets()
        {
            List<Vector2Int> result = new List<Vector2Int>();
            List<Vector2Int> allTargets = (List<Vector2Int>) GetAllTargets();
            List<Vector2Int> reachableTargets = new List<Vector2Int>();
            _notReachebleTarget.Clear();
            foreach (Vector2Int v2 in allTargets)
            {
                if (IsTargetInRange(v2))
                {
                    reachableTargets.Add(v2);
                }
                else
                {
                    _notReachebleTarget.Add(v2);
                }
            }
            if (allTargets.Count == 0 || reachableTargets.Count == 0)
            {
                Vector2Int enemyBase = runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId];
                result.Add(enemyBase);
                return result;
            }
            SortByDistanceToOwnBase(reachableTargets);
            int targetIndex = _unitNumber % _maxUnits;
            result.Add(reachableTargets[targetIndex]);
            return result;
        }

        public override void Update(float deltaTime, float time)
        {
            if (_overheated)
            {
                _cooldownTime += Time.deltaTime;
                float t = _cooldownTime / ( OverheatCooldown / 10 );
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