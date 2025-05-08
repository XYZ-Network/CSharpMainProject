using System;
using System.Collections.Generic;
using GluonGui.Dialog;
using Model;
using Model.Runtime.Projectiles;
using TMPro;
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
        private static int _IdCounter = 0;
        private int _UnitId = _IdCounter++;
        private const int _MaxTarget = 3;
        public List<Vector2Int> Outreachable = new List<Vector2Int>();

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            float GunTemperature = GetTemperature();
            ///////////////////////////////////////
            // Homework 1.3 (1st block, 3rd module)
            ///////////////////////////////////////    
            if (GunTemperature >= overheatTemperature)
            {
                return;
            }
            for (float Multishoot = 0f; Multishoot <= GunTemperature; Multishoot++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }
            IncreaseTemperature();
            ///////////////////////////////////////
        }

        public override Vector2Int GetNextStep()
        {
            Vector2Int target = Outreachable[0];
            if (Outreachable.Count > 0 && !IsTargetInRange(target))
            {
                return unit.Pos.CalcNextStepTowards(target);
            }

            else
            {
                return unit.Pos;
            }
        }

        protected override List<Vector2Int> SelectTargets()
        {
            ///////////////////////////////////////
            // Homework 1.4 (1st block, 4rd module)
            ///////////////////////////////////////
            List<Vector2Int> OurTarget = new List<Vector2Int>();
            Outreachable.Clear();
            
            float minDistance = float.MaxValue;
            Vector2Int NearTarget = Vector2Int.zero;
            
            foreach (var target in GetAllTargets())
            {
                Outreachable.Add(target);
            }

            if (Outreachable.Count == 0)
            {
                int enemyBaseId = IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId;
                Vector2Int enemyBase = runtimeModel.RoMap.Bases[enemyBaseId];
                Outreachable.Add(enemyBase);
            }
            else
            {
                SortByDistanceToOwnBase(Outreachable);
                int targetIndex = _UnitId % _MaxTarget;

                if (targetIndex > (Outreachable.Count - 1))
                {
                    NearTarget = Outreachable[0];
                }
                else
                {
                    if (targetIndex == 0)
                    {
                        NearTarget = Outreachable[targetIndex];
                    }
                    else
                    {
                        NearTarget = Outreachable[targetIndex - 1];
                    }
                }

                if (IsTargetInRange(NearTarget))
                    OurTarget.Add(NearTarget);
            }

            return OurTarget;
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