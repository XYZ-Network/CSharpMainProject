using System.Collections.Generic;
using System.IO;
using System.Linq;
using Model;
using Model.Runtime.Projectiles;
using UnitBrains.Pathfinding;
using UnityEngine;
using UnityEngine.UIElements;
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
        private List<Vector2Int> _dangerTargetNoReachable = new List<Vector2Int>();

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            ///////////////////////////////////////
            // Homework 1.3 (1st block, 3rd module)
            ///////////////////////////////////////
            if (GetTemperature() < overheatTemperature)
            {
                IncreaseTemperature();
            }
            else
            {
                return;
            }


            var quantityProject = GetTemperature();

            for (var i = 0; i < quantityProject; i++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }




            ///////////////////////////////////////
            /*4. В методе GetNextStep() нужно описать получение цели из списка целей. 
             * Если целей там нет или цель в области атаки, нужно вернуть позицию юнита. 
             * Метод уже написан, просто измени реализацию, удалив заглушку return base.GetNextStep().*/
        }

        public override Vector2Int GetNextStep()
        {
            Vector2Int position = unit.Pos;

            if (_dangerTargetNoReachable.Any())
            {
                position = position.CalcNextStepTowards(_dangerTargetNoReachable[0]);
                return position;
            }
            else
            {
                return position;
            }

        }

        protected override List<Vector2Int> SelectTargets()
        {
            _dangerTargetNoReachable.Clear();
            var allTargets = GetAllTargets();

            if (allTargets.Any())
            {
                var dangerTargets = GetDandgerTarget(allTargets);
                var dangerTarget = dangerTargets[0];
                if (IsTargetInRange(dangerTarget))
                    return new List<Vector2Int> {dangerTarget};

                _dangerTargetNoReachable.Add(dangerTarget);
                return new List<Vector2Int>();
            }
            else
            {
                var enemyBase = runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId];

                if (IsTargetInRange(enemyBase))
                {
                    return new List<Vector2Int> { enemyBase };
                }

                _dangerTargetNoReachable.Add(enemyBase);
                return new List<Vector2Int>();

            }
        }

        public List<Vector2Int> GetDandgerTarget(IEnumerable<Vector2Int> targets)
        {
            float maxDistance = float.MaxValue;
            Vector2Int position = Vector2Int.zero;
            var dangerTargets = new List<Vector2Int>();

            foreach (var targetPosition in GetAllTargets())
            {
                float distanceToOwnBase = DistanceToOwnBase(targetPosition);
                if (distanceToOwnBase < maxDistance)
                {
                    maxDistance = distanceToOwnBase;
                    position = targetPosition;
                }
            }
            dangerTargets.Add(position);
            return dangerTargets;
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