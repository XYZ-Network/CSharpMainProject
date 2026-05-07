using BuffSystem;
using Model;
using Model.Runtime.Projectiles;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnitBrains.Pathfinding;
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
        private List<Vector2Int> UnreachableTargets = new List<Vector2Int>();
        private static int CounterID = 0;
        private int UnitID;
        private const int MaxID = 3;

        private BaseUnitPath _activePath = null;
        //public override BaseUnitPath ActivePath => _activePath; //закоментировать чтобы убрать отображение пути для юнита

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            if (GetTemperature() < overheatTemperature)
            {
                if (GetTemperature() == 0)
                {
                    var projectile = CreateProjectile(forTarget);
                    AddProjectileToList(projectile, intoList);
                }
                else
                {
                    for (float T = 0f; T <= GetTemperature(); T++)
                    {
                        var projectile = CreateProjectile(forTarget);
                        AddProjectileToList(projectile, intoList);
                    }
                }
                IncreaseTemperature();
            }
        }

        public override Vector2Int GetNextStep()
        {
            if (UnreachableTargets.Count == 0 || GetReachableTargets().Contains(UnreachableTargets[0]))
                return unit.Pos;
            else
            {
                var target = UnreachableTargets[0];
                _activePath = new NewUnitPath(runtimeModel, unit.Pos, target);
                return _activePath.GetNextStepFrom(unit.Pos);
            }
        }

        protected override List<Vector2Int> SelectTargets()
        {
            UnreachableTargets.Clear();
            List<Vector2Int> result = GetAllTargets().ToList();

            SortByDistanceToOwnBase(result);

            Vector2Int NearestTarget = new Vector2Int();

            UnitID = CounterID;
            CounterID++;

            if (result.Count >= 1)
            {
                foreach (var target in result)
                {
                    int ID;
                    if (result.Count > MaxID)
                    {
                        ID = UnitID % MaxID;
                    }
                    else
                    {
                        ID = UnitID % result.Count;
                    }
                    NearestTarget = result[ID];
                }
                result.Clear();
                UnreachableTargets.Add(NearestTarget);
                if (GetReachableTargets().Contains(NearestTarget))
                {
                    result.Add(NearestTarget);
                }
            }

            if (result.Count == 0)
            {
                var enemyBase = runtimeModel.RoMap.Bases[1];
                result.Clear();
                if (GetReachableTargets().Contains(enemyBase))
                    result.Add(enemyBase);
                else UnreachableTargets.Add(enemyBase);
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