using Model;
using Model.Runtime.Projectiles;
using System.Collections.Generic;
using System.Linq;
using UnitBrains.Pathfinding;
using UnityEngine;
using UnityEngine.UIElements;
using Utilities;
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
            int EnemyNumber = (_id - 1) % numberOfEnemy;
            List <Vector2Int> targets = new List<Vector2Int>();
            targets = GetAllTargets().ToList();
            SortByDistanceToOwnBase(targets);
            Vector2Int target = targets[EnemyNumber];

            _activePath = new UnitPath(runtimeModel, position, target);

            if (IsTargetInRange(target))
            {
                return position;
            }

            return _activePath.GetNextStepFrom(position);
        }

        protected override List<Vector2Int> SelectTargets()
        {
            ///////////////////////////////////////
            // Homework 1.4 (1st block, 4rd module)
            ///////////////////////////////////////
            List<Vector2Int> targets = GetAllTargets().ToList();
            SortByDistanceToOwnBase(targets);

            List <Vector2Int> Result = new List<Vector2Int>();

            int numberOfEnemy = GetAllTargets().Count();
            int EnemyNumber = (_id - 1) % numberOfEnemy;


            if (IsTargetInRange(targets[EnemyNumber]))
            {
                Result.Add(targets[EnemyNumber]);
            }
            
            return Result;
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