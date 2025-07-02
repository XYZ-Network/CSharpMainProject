using Model;
using Model.Runtime.Projectiles;
using System.Collections.Generic;
using System.Linq;
using UnitBrains.Pathfinding;
using UnityEngine;

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
        private List<Vector2Int> _outOfRangeTargets = new();
        private static int _unitCounter = 0;
        private int _unitId = _unitCounter++;
        private const int _maximumSelectionTargets = 3;
        private AStarUnitPath _activePath;
        private Vector2Int _lastTarget;
        private Vector2Int _lastStart;

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;

            if (GetTemperature() >= overheatTemperature) // проверка температуры
            {
                Debug.Log("перегрев");
                return;
            }

            int currentTemp = GetTemperature();           // 0…3
            int shots = (currentTemp + 1) * unit.CurrentProjectileMultiplier;

            for (int i = 0; i < shots; i++)                // теперь ×2, если висит DoubleShot
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
                //Debug.Log($"Выстрел {i}, температура: {currentTemp}");
            }
            IncreaseTemperature(); // нагрев после выстрела
        }

        public override Vector2Int GetNextStep()
        {
            var inRangeTargets = SelectTargets();

            if (inRangeTargets.Count > 0)
            {
                return unit.Pos;
            }

            if (_outOfRangeTargets.Count > 0)
            {
                Vector2Int target = _outOfRangeTargets[0];

                if (_activePath == null || _lastTarget != target || _lastStart != unit.Pos)
                {
                    _activePath = new AStarUnitPath(runtimeModel, unit.Pos, target);
                    VisualizePath(_activePath);
                    _lastTarget = target;
                    _lastStart = unit.Pos;
                }
                return _activePath.GetNextStepFrom(unit.Pos);
            }
            return unit.Pos;
        }

        protected override List<Vector2Int> SelectTargets()
        {
            _outOfRangeTargets.Clear();

            List<Vector2Int> allTargets = GetAllTargets().ToList();
            List<Vector2Int> result = new();

            var coord = Coordinator;
            var recPoint = coord.RecommendedPoint;
            bool occupied = runtimeModel.RoUnits.Any(u => u.Pos == recPoint);

            if (!IsTargetInRange(coord.RecommendedTarget)
                && recPoint != unit.Pos
                && runtimeModel.IsTileWalkable(recPoint)
                && !occupied)
            {
                _outOfRangeTargets.Add(recPoint);
            }

            if (allTargets.Count == 0)
            {
                Vector2Int enemyBase = runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId];
                allTargets.Add(enemyBase);
            }

            SortByDistanceToOwnBase(allTargets);

            int indexTarget = _unitId % _maximumSelectionTargets;

            if (indexTarget >= allTargets.Count)
            {
                indexTarget = allTargets.Count - 1;
            }

            Vector2Int selectedTarget = allTargets[indexTarget];

            (IsTargetInRange(selectedTarget)
                ? result
                : _outOfRangeTargets
                ).Add(selectedTarget);

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