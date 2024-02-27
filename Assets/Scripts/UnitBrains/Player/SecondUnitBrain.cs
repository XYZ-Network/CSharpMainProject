﻿using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Model.Runtime;
using Model.Runtime.Projectiles;
using UnityEngine;
using Model;

namespace UnitBrains.Player
{
    public class SecondUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Cobra Commando";
        private const float OverheatTemperature =4f;
        private const float OverheatCooldown = 0f;
        private float _temperature = 0f;
        private float _cooldownTime = 0f;
        private bool _overheated;
        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;         
            float temperat = GetTemperature();
            if (temperat >= overheatTemperature) return;
            for (float strel = 1f; strel <= temperat; strel += 1f)
            {            
                    var projectile = CreateProjectile(forTarget);
                    AddProjectileToList(projectile, intoList);
            }
            IncreaseTemperature();
        }
        protected override List<Vector2Int> SelectTargets()
        {
            List<Vector2Int> result = new List<Vector2Int>();
            List<Vector2Int> _allTargets = new List<Vector2Int>();
            float min= float.MaxValue;
            Vector2Int near = Vector2Int.zero;
            if (result.Count == 1)
            {
                return result;
            }
            foreach (Vector2Int i in GetAllTargets())
            {
                float Distance = DistanceToOwnBase(i);
                if (Distance < min) 
                {
                    min = Distance;
                    near = i;
                }              
                foreach (var target in GetAllTargets())
                {
                    _allTargets.Add(target);
                }              
                if (min < float.MaxValue)
                {
                    if (IsTargetInRange(near))
                    {
                        result.Add(near);
                    }
                    else
                    {
                        int palyerID = IsPlayerUnitBrain ? RuntimeModel.PlayerId : RuntimeModel.BotPlayerId;
                        Vector2Int enemyBase = runtimeModel.RoMap.Bases[palyerID];                        
                    }
                }
            }
            result.Clear();
            result.Add(near);
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