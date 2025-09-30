using System.Collections.Generic;
using System.Linq;
using Codice.CM.Common.Tree.Partial;
using GluonGui.Dialog;
using Model;
using Model.Runtime.Projectiles;
using Unity.VisualScripting;
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
        private List<Vector2Int> TargetsOutOfRange = new();
        private static int UnitCounter = 0;
        private int UnitID = UnitCounter++;
        private const int MaxUnitID = 3;
        
        
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
            //Vector2Int target = TargetsOutOfRange.Count > 0 ? TargetsOutOfRange[0] : unit.Pos;
            //if (IsTargetInRange(target))
            //{
            //    return unit.Pos;
            //}
            //else
            //{
            //    return unit.Pos.CalcNextStepTowards(target);
            ////}

            //bool HasTargetsInDoubleRange() //тут начало
            //{
            //    var attackRangeSqr = (unit.Config.AttackRange * unit.Config.AttackRange) * 2;
            //    foreach (var possibleTarget in GetAllTargets())
            //    {
            //        var diff = possibleTarget - unit.Pos;
            //        if (diff.sqrMagnitude < attackRangeSqr)
            //            return true;
            //    }

            //    return false;
            //}

            //if (HasTargetsInDoubleRange())
            //{
            //    return UnitCoordinator.GetInstance().GetTarget();
            //}

            //return UnitCoordinator.GetInstance().GetPoint(); //Тут конец

            return base.GetNextStep();        //Раньше только это было

        }

        protected override List<Vector2Int> SelectTargets()
        {
            List<Vector2Int> result = new List<Vector2Int>();
            TargetsOutOfRange.Clear();
            
            foreach (var target in GetAllTargets())
            {
                TargetsOutOfRange.Add(target);
            }

            if (TargetsOutOfRange.Count == 0)
            {
                TargetsOutOfRange.Add(runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId]);
            }

            SortByDistanceToOwnBase(TargetsOutOfRange);

            int targetNum = UnitID % MaxUnitID;
            int bestTargetNum = Mathf.Min(targetNum, TargetsOutOfRange.Count - 1);
            Vector2Int bestTarget = TargetsOutOfRange[bestTargetNum];

            if(IsTargetInRange(bestTarget)) result.Add(bestTarget);

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