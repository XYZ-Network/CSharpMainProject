﻿using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Model;
using Model.Runtime.Projectiles;
using Unity.VisualScripting;
using UnityEngine;
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
        private List<Vector2Int> _outOfRangeTargets = new List<Vector2Int>();

        private static int _unitIndexer = 0;
        public int UnitID { get; private set; }
        private const int MAXIMUM_TARGETS = 4;

        public SecondUnitBrain()
        {
            if(GetAllPlayerUnits().ToList().Count == 0)
                _unitIndexer = 0;
            UnitID = _unitIndexer;
            _unitIndexer++;
        }

        
        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            float temp = GetTemperature();

            if (temp >= overheatTemperature)
                return;

            IncreaseTemperature();

            for (int i = 0; i <= temp; i++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }
        }

        public override Vector2Int GetNextStep()
        {
            if (_outOfRangeTargets.Any())
            {
                var target = CalcNextStepTowards(_outOfRangeTargets[_outOfRangeTargets.Count - 1]);
                return target;
            }

            return unit.Pos;
        }

        protected override List<Vector2Int> SelectTargets()
        {
            #region Prev. version
            /*List<Vector2Int> result = GetReachableTargets();

            if (result.Any())
            {
                Vector2Int target = new Vector2Int();
                float minDistanceToBase = float.MaxValue;

                foreach (var element in result)
                {
                    float currentElementDistance = DistanceToOwnBase(element);
                    if (currentElementDistance < minDistanceToBase)
                    {
                        minDistanceToBase = currentElementDistance;
                        target = element;
                    }
                }
                result.Clear();
                result.Add(target);
            }

            return result;*/
            #endregion
            #region Prev. Version
            List<Vector2Int> result = new List<Vector2Int>();
            List<Vector2Int> allTargets = GetAllTargets().ToList();
            List<Vector2Int> reachables = GetReachableTargets();

            if (allTargets.Count > 1)
            {
                allTargets.Remove(runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId]);
            }

            if (allTargets.Any())
            {
                Vector2Int target = new Vector2Int();
                SortByDistanceToOwnBase(allTargets);

                if (UnitID < MAXIMUM_TARGETS && UnitID < allTargets.Count)
                {
                    target = allTargets[UnitID];
                }
                else
                {
                    if (allTargets.Count <= MAXIMUM_TARGETS)
                    {
                        target = allTargets[allTargets.Count - 1];
                    }
                    else
                    {
                        target = allTargets[MAXIMUM_TARGETS];
                    }
                }

                if (reachables.Contains(target))
                {
                    result.Add(target);
                    return result;
                }
                else
                {
                    _outOfRangeTargets.Add(target);
                }
            }

            return result;
            #endregion
        }

        private Vector2Int GetMostWantedTarget(List<Vector2Int> enemyUnits)
        {
            Vector2Int target = new Vector2Int();
            float minDistanceToBase = float.MaxValue;

            foreach (var unit in enemyUnits)
            {
                float currentElementDistance = DistanceToOwnBase(unit);
                if (currentElementDistance < minDistanceToBase)
                {
                    minDistanceToBase = currentElementDistance;
                    target = unit;
                }
            }

            return target;
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