using Model;
using Model.Runtime.Projectiles;
using System.Collections.Generic;
using System.Linq;
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
        public static int ID = 0;
        private int _id = ID++;
        
        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            ///////////////////////////////////////
            // Homework 1.3 (1st block, 3rd module)
            ///////////////////////////////////////
            if (GetTemperature() < overheatTemperature)
            {
                for (int i = 0; i < GetTemperature() + 1; i++)
                {
                    var projectile = CreateProjectile(forTarget);
                    AddProjectileToList(projectile, intoList);
                }
                IncreaseTemperature();
            }
            else
            {
                return;
            }
            ///////////////////////////////////////
        }

        public override Vector2Int GetNextStep()
        {
            Vector2Int position = unit.Pos;
            int numberOfEnemy = GetAllTargets().Count();
            int EnemyNumber = _id % numberOfEnemy;
            Vector2Int nextPosition = GetAllTargets().ToList()[EnemyNumber];
            if (IsTargetInRange(nextPosition))
            {
                return position;
            }
            return position.CalcNextStepTowards(nextPosition);
        }

        protected override List<Vector2Int> SelectTargets()
        {
            ///////////////////////////////////////
            // Homework 1.4 (1st block, 4rd module)
            ///////////////////////////////////////
            List<Vector2Int> result = GetAllTargets().ToList();
            SortByDistanceToOwnBase(result);
            List<Vector2Int> BodiesNotInRange = new List<Vector2Int>();
            Vector2Int Result = result[0];

            float minima = float.MaxValue;
            int numberOfEnemy = GetAllTargets().Count();
            int EnemyNumber = _id % numberOfEnemy;


            if (result.Count() == 0)
            {
                int enemyId = IsPlayerUnitBrain ? RuntimeModel.PlayerId : RuntimeModel.BotPlayerId;
                result.Clear();
                if (IsTargetInRange(runtimeModel.RoMap.Bases[enemyId]))
                    result.Add(runtimeModel.RoMap.Bases[enemyId]);
                return result;
            }
            else
            {
                Result = result[EnemyNumber];
                result.Clear();
                if (IsTargetInRange(Result))
                {
                    result.Add(Result);
                }
                

            }
            return result;
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