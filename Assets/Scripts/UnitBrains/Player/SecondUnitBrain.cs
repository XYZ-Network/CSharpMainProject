using System;
using System.Collections.Generic;
using System.Linq;
using Model;
using Model.Runtime.Projectiles;
using TMPro;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
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
        private static int _idUnit = 0;
        private const int _maxTargets = 3;

        public int Id { get; private set; }

        public List<Vector2Int> OutOfRange = new();


        public SecondUnitBrain()
        {
            Id = ++_idUnit;
            Debug.Log($"Unit Counter: {Id}");
        }



        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            float t = GetTemperature();
            IncreaseTemperature();
            if (t >= overheatTemperature)
            {
                return;
            }

            for (float i = GetTemperature(); i <= 3 && i > 0; i--)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
                if (unit.DoubleShootMode)
                {
                    var projectileAnother = CreateProjectile(forTarget);
                    AddProjectileToList(projectileAnother, intoList);
                }
            }
            IncreaseTemperature();

        }

        public override Vector2Int GetNextStep()
        {

            Vector2Int target;
            target = OutOfRange.Count > 0 ? OutOfRange[0] : unit.Pos;
            return IsTargetInRange(target) ? unit.Pos : unit.Pos.CalcNextStepTowards(target);

        }


        protected override List<Vector2Int> SelectTargets()
        {
            List<Vector2Int> result = new List<Vector2Int>();
            OutOfRange.Clear();

            foreach (var target in GetAllTargets())
            {
                OutOfRange.Add(target);
            }

            if (OutOfRange.Count == 0)
            {
                OutOfRange.Add(runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId]);
            }

            SortByDistanceToOwnBase(OutOfRange);

            int numberPromisingGoals = OutOfRange.Count <= _maxTargets ? OutOfRange.Count : _maxTargets;
            int targetIndex = Id % numberPromisingGoals;
            Vector2Int targetNumL = OutOfRange[targetIndex];

            if (IsTargetInRange(targetNumL))
            {
                result.Add(targetNumL);
                OutOfRange.Clear();
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