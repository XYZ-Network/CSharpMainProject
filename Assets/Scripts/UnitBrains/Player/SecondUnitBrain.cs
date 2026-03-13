using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using JetBrains.Annotations;
using log4net.Util;
using Model;
using Model.Runtime.Projectiles;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
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

        private readonly List<Vector2Int> _currTarget = new();

        private const int ClosestTargetsToAttack = 4;
        private static int _instanceCounter = 0;
        private int _unitNumber = _instanceCounter++;

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
            var target = _currTarget.Count > 0 ? _currTarget[0] : unit.Pos;
            return IsTargetInRange(target) ? unit.Pos : unit.Pos.CalcNextStepTowards(target); 
        }
        

        protected override List<Vector2Int> SelectTargets()
        {
            List<Vector2Int> result = new List<Vector2Int>();
            _currTarget.Clear();
            foreach (var target in GetAllTargets())
            {
                _currTarget.Add(target);
            }

            if (_currTarget.Count == 0)
            {
                _currTarget.Add(runtimeModel.RoMap.Bases[
                    IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId]);
            }
            SortByDistanceToOwnBase(_currTarget);


            var myTargetNum = _unitNumber % ClosestTargetsToAttack;
            var targetNum = Mathf.Min(myTargetNum, _currTarget.Count - 1);
            var bestTarget = _currTarget[targetNum];
            if (IsTargetInRange(bestTarget))
            {
                result.Add(bestTarget);
            }

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