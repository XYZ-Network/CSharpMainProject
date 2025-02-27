using System.Collections.Generic;
using System.Linq;
using GluonGui.Dialog;
using Model;
using Model.Runtime.Projectiles;
using UnitBrains.Pathfinding;
using UnityEngine;
using Utilities;
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

        private List<Vector2Int> OutOfReachTarget = new List<Vector2Int>();

        private static int _counter = -1;
        private const int _targetMaxNum = 3;
        private int _unitNum;

        public SecondUnitBrain()
        {
            _unitNum = _counter;
            Debug.Log(_unitNum);
            _counter++;
        }
        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            ///////////////////////////////////////
            // Homework 1.3 (1st block, 3rd module)
            ///////////////////////////////////////           
            if (GetTemperature() >= overheatTemperature) return;
            else
            {
                IncreaseTemperature();
                for (int i = 0; i < GetTemperature(); i++) 
                {
                    var projectile = CreateProjectile(forTarget);
                    AddProjectileToList(projectile, intoList);
                }
                if (unit.DubleStrike)
                {
                    Debug.LogWarning("DubleStike");
                    for (int i = 0; i < GetTemperature(); i++)
                    {
                        var projectile = CreateProjectile(forTarget);
                        AddProjectileToList(projectile, intoList);
                    }
                }

            }
            
            ///////////////////////////////////////
        }

        public override Vector2Int GetNextStep()
        {
            BaseUnitPath result;
            if (OutOfReachTarget.Count > 0)
            {
                result = new MyUnitPath(runtimeModel, IsPlayerUnitBrain, unit.Pos, OutOfReachTarget[0]);
                return result.GetNextStepFrom(unit.Pos);
            }
            else return unit.Pos;
            
        }

        protected override List<Vector2Int> SelectTargets()
        {
            ///////////////////////////////////////
            // Homework 1.4 (1st block, 4rd module)
            ///////////////////////////////////////
            List<Vector2Int> result = new List<Vector2Int>();
            List<Vector2Int> allTarget = GetAllTargets().ToList();

            if (allTarget.Count > 0)
            {
                SortByDistanceToOwnBase(allTarget);
                if(allTarget.Count < _counter + 1)
                {
                    result.Add(allTarget[0]);
                }
                else result.Add(allTarget[_unitNum % _targetMaxNum]);

                if (IsTargetInRange(result[0]))
                {
                    OutOfReachTarget.Clear();
                    return result;
                }
                else
                {
                    OutOfReachTarget.Add(result[0]);
                    result.Clear();
                    return result;
                }
            }
            else
            {
                Vector2Int botBase = runtimeModel.RoMap.Bases[0];

                if (IsTargetInRange(botBase))
                {
                    OutOfReachTarget.Clear();
                    result.Add(botBase); 
                    return result;

                }
                else
                {
                    OutOfReachTarget.Add(botBase);
                    return result;
                }
            }
            
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