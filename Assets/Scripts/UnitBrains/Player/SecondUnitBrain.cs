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
        
        /////////////////////////////////////////////////////////
        public static int CounterUnit { get; private set; } = 0;
        public int NumberUnit { get; set; }
        public const int MaxUnitTargetToAttack = 3;
        /////////////////////////////////////////////////////////

        public SecondUnitBrain()
        {
            if (CounterUnit == MaxUnitTargetToAttack - 1)
                NumberUnit = MaxUnitTargetToAttack - 1;

            NumberUnit = CounterUnit;
            CounterUnit++;
        }

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
            List<Vector2Int> targets = SelectTargets();
            
            if (targetsOutRangeUnit.Count > 0)
            {
                Vector2Int position = unit.Pos;
                Vector2Int nextPosition = targetsOutRangeUnit[NumberUnit % targetsOutRangeUnit.Count];
                return position.CalcNextStepTowards(nextPosition);
            }
            return targets[NumberUnit % targets.Count];
        }

        protected override List<Vector2Int> SelectTargets()
        {
            List<Vector2Int> targets = GetAllTargets().ToList();
            SortByDistanceToOwnBase(targets);
            List<Vector2Int> result = new();

            if (targets != null)
            {
                for (int i = 0; i < targets.Count; i++)
                {
                    if (i == MaxUnitTargetToAttack)
                        break;
                    else if (IsTargetInRange(targets[i]))
                        result.Add(targets[i]);
                    else
                    {
                        targetsOutRangeUnit.Clear();
                        targetsOutRangeUnit.Add(targets[i]);
                    }
                }
            }
            else
            {
                targetsOutRangeUnit.Clear();
                targetsOutRangeUnit.Add(runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId]);
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