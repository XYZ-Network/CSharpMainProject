using System.Collections.Generic;
using System.Linq;
using Model;
using Model.Runtime.Projectiles;
using UnityEngine;
using Utilities;

namespace UnitBrains.Player
{
    public class ThirdUnitBrain : DefaultPlayerUnitBrain
    {
        private const float FireCooldown = 0.5f; // ¬рем€ между атаками
        private float fireTimer = 0f;

        private const float OverheatTemperature = 3f;
        private const float OverheatCooldown = 2f;

        private float _temperature = 0f;
        private float _cooldownTime = 0f;
        private bool _overheated;

        private List<Vector2Int> _targetsToMove = new List<Vector2Int>();

        // Ќомер юнита, чтобы распредел€ть цели (можно унаследовать из второго мозга, если нужно)
        private static int _unitCounter = 0;
        private readonly int _unitNumber;

        public ThirdUnitBrain()
        {
            _unitNumber = _unitCounter++;
        }

        public override string TargetUnitName => "Third Unit";

        public override Vector2Int GetNextStep()
        {
            if (_targetsToMove.Count == 0)
                return unit.Pos;

            return unit.Pos.CalcNextStepTowards(_targetsToMove[0]);
        }

        protected override List<Vector2Int> SelectTargets()
        {
            List<Vector2Int> result = new List<Vector2Int>();
            _targetsToMove.Clear();

            var allTargets = GetAllTargets().ToList();
            SortByDistanceToOwnBase(allTargets);

            if (allTargets.Count > 0)
            {
                int targetIndex = Mathf.Min(_unitNumber, allTargets.Count - 1);
                Vector2Int selectedTarget = allTargets[targetIndex];

                if (IsTargetInRange(selectedTarget))
                    result.Add(selectedTarget);
                else
                    _targetsToMove.Add(selectedTarget);
            }
            else
            {
                int enemyBaseId = IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId;
                Vector2Int enemyBase = runtimeModel.RoMap.Bases[enemyBaseId];
                _targetsToMove.Add(enemyBase);
            }

            return result;
        }

        // ѕростейша€ сортировка целей по дистанции до своей базы
        private void SortByDistanceToOwnBase(List<Vector2Int> targets)
        {
            targets.Sort((a, b) =>
            {
                float distA = DistanceToOwnBase(a);
                float distB = DistanceToOwnBase(b);
                return distA.CompareTo(distB);
            });
        }

        public override void Update(float deltaTime, float time)
        {
            fireTimer += deltaTime;

            if (_overheated)
            {
                _cooldownTime += deltaTime;
                float t = _cooldownTime / OverheatCooldown;
                _temperature = Mathf.Lerp(OverheatTemperature, 0, t);
                if (t >= 1f)
                {
                    _cooldownTime = 0;
                    _overheated = false;
                }
            }
        }

        // ¬нешний метод дл€ получени€ снар€дов, которые надо заспавнить в текущем кадре
        public List<BaseProjectile> GetProjectilesToSpawn()
        {
            var projectiles = new List<BaseProjectile>();

            if (fireTimer < FireCooldown)
                return projectiles;

            if (!HasTargetsInRange())
                return projectiles;

            if (GetTemperature() >= OverheatTemperature)
                return projectiles;

            IncreaseTemperature();

            var targets = SelectTargets();
            foreach (var target in targets)
            {
                for (int i = 0; i < GetTemperature(); i++)
                {
                    var projectile = CreateProjectile(target);
                    projectiles.Add(projectile);
                }
            }

            fireTimer = 0f;
            return projectiles;
        }

        private int GetTemperature()
        {
            if (_overheated)
                return (int)OverheatTemperature;
            else
                return (int)_temperature;
        }

        private void IncreaseTemperature()
        {
            _temperature += 1f;
            if (_temperature >= OverheatTemperature)
                _overheated = true;
        }
    }
}

