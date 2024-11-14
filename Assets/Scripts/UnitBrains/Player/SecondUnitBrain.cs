using System.Collections.Generic;
using System.Linq;
using Model;
using Model.Runtime.Projectiles;
using UnityEngine;
using Utilities;

namespace UnitBrains.Player
{
    public class SecondUnitBrain : DefaultPlayerUnitBrain
    {
        public SecondUnitBrain()
        {
            Id = IdBilling++;
        }

        public static int IdBilling = 0;
        public int Id;

        public override string TargetUnitName => "Cobra Commando";

        private const float OverheatTemperature = 3f;
        private const float OverheatCooldown = 2f;
        private const int MaxTarget = 3;

        private float _temperature = 0f;
        private float _cooldownTime = 0f;
        private bool _overheated;

        private Vector2Int _notRangeEnemyPosition;
        private List<Vector2Int> allTargetEnemies = new List<Vector2Int>();

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            //float overheatTemperature = OverheatTemperature;
            // Homework 1.3 (1st block, 3rd module)
            if (!_overheated)
            {
                for (float i = 0; i <= GetTemperature(); i++)
                {
                    var projectile = CreateProjectile(forTarget);
                    AddProjectileToList(projectile, intoList);
                }
                IncreaseTemperature();
            }

            ///////////////////////////////////////
        }

        public override Vector2Int GetNextStep()
        {
            return unit.Pos.CalcNextStepTowards(_notRangeEnemyPosition);
        }

        protected override List<Vector2Int> SelectTargets()
        {
            // Homework 1.4 (1st block, 4rd module)
            allTargetEnemies.Clear();
            allTargetEnemies = GetAllTargets().ToList();

            Vector2Int targetBaseEnemy = runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId];

            allTargetEnemies.Remove(targetBaseEnemy);

            if (allTargetEnemies.Count > 0)
            {
                SortByDistanceToOwnBase(allTargetEnemies);

                if (allTargetEnemies.Count > MaxTarget)
                {
                    allTargetEnemies = allTargetEnemies.GetRange(0, MaxTarget);
                }

                int idEnemy = (Id - 1) % allTargetEnemies.Count;

                Vector2Int target = allTargetEnemies[idEnemy];

                allTargetEnemies.Clear();

                if (!HasTargetsInRange())
                {
                    _notRangeEnemyPosition = target;
                }
                else
                {
                    allTargetEnemies.Add(target);
                }
            }
            else
            {
                allTargetEnemies.Clear();

                if (!HasTargetsInRange())
                {
                    _notRangeEnemyPosition = targetBaseEnemy;
                }
                else
                {

                    allTargetEnemies.Add(targetBaseEnemy);
                }
            }

            return allTargetEnemies;
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