using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using JetBrains.Annotations;
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
            List<Vector2Int> result = new List<Vector2Int>();

            _outOfRangeTargets.Clear();

            foreach (var target in GetAllTargets())
            {
                _outOfRangeTargets.Add(target);
            }

            if(_outOfRangeTargets.Count == 0)
            {
                _outOfRangeTargets.Add(runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId]);
            }

            SortByDistanceToOwnBase(_outOfRangeTargets);

            int targetNum = UnitID % MAXIMUM_TARGETS;
            int bestTargetNum = Mathf.Min(targetNum, _outOfRangeTargets.Count - 1);
            Vector2Int bestTaget = _outOfRangeTargets[bestTargetNum];
            if (IsTargetInRange(bestTaget))
            {
                result.Add(bestTaget);
                _outOfRangeTargets.Clear();
            }
            return result;
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