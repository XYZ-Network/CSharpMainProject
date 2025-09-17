using Model;
using Model.Runtime;
using Model.Runtime.Projectiles;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
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

        private List<Vector2Int> _targetsOutOfRange = new List<Vector2Int>();

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            int temperature = GetTemperature();

            if (temperature >= overheatTemperature)
            {
                return;
            }

            for (int i = 0; i <= temperature; ++i)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }

            IncreaseTemperature();
        }

        public override Vector2Int GetNextStep()
        {
            if (_currentMovementTarget == null) return unit.Pos;

            return unit.Pos.CalcNextStepTowards(_currentMovementTarget.Value);
            //return base.GetNextStep();
        }

        private Vector2Int? _currentMovementTarget;
        private List<Vector2Int> _reachableTargets = new List<Vector2Int>();
        private static int _countTargets = 0;
        private int _numberUnit;
        private const int _maxTargetsToChoose = 3;

        protected override List<Vector2Int> SelectTargets()
        {
            if (_numberUnit == 0)
            {
                _numberUnit = _countTargets;
                _countTargets++;
            }

            _targetsOutOfRange.Clear();
            List<Vector2Int> result = new List<Vector2Int>();

            // ---------------------ДЗ 5 (2.3)---------------------
            // Очистка списка. Запись всех целей в очищенный список.
            _reachableTargets.Clear();
            foreach (var target in GetAllTargets().ToList())
            {
                _reachableTargets.Add(target);
            }

            if (_reachableTargets.Count > 0)
            {
                // Сортировка целей по дистанции
                SortByDistanceToOwnBase(_reachableTargets);
                int targetIndex = _numberUnit % _maxTargetsToChoose;

                if (targetIndex >= _reachableTargets.Count) targetIndex = _reachableTargets.Count - 1; ;

                Vector2Int selectedTarget = _reachableTargets[targetIndex];

                if (IsTargetInRange(selectedTarget))
                {
                    result.Add(selectedTarget);
                    _currentMovementTarget = null;
                }
                else
                {
                    _currentMovementTarget = selectedTarget;
                    _targetsOutOfRange.Add(selectedTarget);
                }
            }
            else
            {
                _currentMovementTarget = runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId];
            }

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