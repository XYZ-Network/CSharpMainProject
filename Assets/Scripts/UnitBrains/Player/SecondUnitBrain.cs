using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Model;
using Model.Runtime.Projectiles;
using Unity.VisualScripting;
using UnityEngine;
using Assets.Scripts.Utilities;
using Model.Runtime.ReadOnly;
using UnityEngine.UIElements;

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
        public List<Vector2Int> allTargets = new List<Vector2Int>();
        public List<Vector2Int> Bases = new List<Vector2Int>();
        public Vector2Int _currentTarget;
        

        public static int Counter = 0;
        const int MaxTargets = 4;
        private int _unitID = Counter++;


        
        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            float temp = GetTemperature();

            if (temp >= overheatTemperature)
            {
                return;
            }
            IncreaseTemperature();


            for (int i = 0; i <= temp; i++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }

        }

        public override Vector2Int GetNextStep()
        {
            if (allTargets.Any())
            {
                if (IsTargetInRange(allTargets[0]))
                {
                    return unit.Pos;
                }
                else
                { 
                    return base.GetNextStep();
                }

            }

            return unit.Pos;
        }

        protected override List<Vector2Int> SelectTargets()
        {
            Bases.Add(runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId]);
            Bases.Add(runtimeModel.RoMap.Bases[RuntimeModel.PlayerId]);
            List<Vector2Int> result = new List<Vector2Int>();
            allTargets.Clear();

            foreach (Vector2Int i in GetAllTargets())
            {
                allTargets.Add(i);
            }
            if (allTargets.Count == 0)
            {
                allTargets.Add(runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId]);
            }

            SortByDistanceToOwnBase(allTargets);

            int TargetNum = _unitID % MaxTargets;
            Vector2Int bestTarget = allTargets[TargetNum];

            Vector2Int recomendedTarget = PosUtil.UnitUtil(allTargets, Bases, bestTarget, _currentTarget);

            if(recomendedTarget != bestTarget)
                bestTarget = recomendedTarget;

            if (IsTargetInRange(bestTarget)) result.Add(bestTarget);
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