using System.Collections.Generic;
using Model.Runtime.Projectiles;
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

        private List<Vector2Int> _targetsToMove = new List<Vector2Int>();

        protected override void GenerateProjectiles(
            Vector2Int forTarget,
            List<BaseProjectile> intoList)
        {
            if (_temperature >= OverheatTemperature)
            {
                _overheated = true;
                return;
            }

            IncreaseTemperature();

            int shots = (int)_temperature + 1;

            for (int i = 0; i < shots; i++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }
        }

        protected override List<Vector2Int> SelectTargets()
        {
            List<Vector2Int> allTargets = GetAllTargets();
            List<Vector2Int> result = new List<Vector2Int>();

            _targetsToMove.Clear();

            if (allTargets.Count == 0)
            {
                int enemyPlayerId = IsPlayerUnitBrain
                    ? RuntimeModel.BotPlayerId
                    : RuntimeModel.PlayerID;

                Vector2Int enemyBase =
                    runtimeModel.RoMap.Bases[enemyPlayerId];

                _targetsToMove.Add(enemyBase);

                return result;
            }

            Vector2Int nearestTarget = allTargets[0];
            float minDistance = DistanceToOwnBase(nearestTarget);

            foreach (Vector2Int target in allTargets)
            {
                float distance = DistanceToOwnBase(target);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestTarget = target;
                }
            }

            _targetsToMove.Add(nearestTarget);

            List<Vector2Int> reachableTargets = GetReachableTargets();

            if (reachableTargets.Contains(nearestTarget))
            {
                result.Add(nearestTarget);
            }

            return result;
        }

        public override Vector2Int GetNextStep()
        {
            if (_targetsToMove.Count == 0)
            {
                return Position;
            }

            Vector2Int target = _targetsToMove[0];

            if (GetReachableTargets().Contains(target))
            {
                return Position;
            }

            return Position.CalcNextStepTowards(target);
        }

        public override void Update(float deltaTime, float time)
        {
            if (_overheated)
            {
                _cooldownTime += Time.deltaTime;

                float t = _cooldownTime / (OverheatCooldown / 10);

                _temperature = Mathf.Lerp(
                    OverheatTemperature,
                    0,
                    t);

                if (t >= 1)
                {
                    _cooldownTime = 0;
                    _overheated = false;
                }
            }
        }

        private int GetTemperature()
        {
            if (_overheated)
                return (int)OverheatTemperature;

            return (int)_temperature;
        }

        private void IncreaseTemperature()
        {
            _temperature += 1f;

            if (_temperature >= OverheatTemperature)
            {
                _overheated = true;
            }
        }
    }
}
