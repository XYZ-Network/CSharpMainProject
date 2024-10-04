using System.Collections.Generic;
using System.Linq;
using Model;
using Model.Runtime.Projectiles;
using UnityEngine;
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
        public Vector2Int _unreacheableEnemy;
        private List<Vector2Int> _vector2s = new List<Vector2Int>();
        
        
        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            ///////////////////////////////////////
            // Homework 1.3 (1st block, 3rd module)
            ///////////////////////////////////////           
            

            if (GetTemperature() >= overheatTemperature)
            {
                return;

            }

            IncreaseTemperature();

            for (int x = 1; x<=GetTemperature(); x++)
            {
                    var projectile = CreateProjectile(forTarget);
                    AddProjectileToList(projectile, intoList);
            }



            
            //Debug.Log(GetTemperature());
            ///////////////////////////////////////
        }

        public override Vector2Int GetNextStep()
        {

      
            

                
                if (_vector2s.Count > 0)
                {
                  
                    _unreacheableEnemy = _unreacheableEnemy.CalcNextStepTowards(_vector2s[0]);

                    return _unreacheableEnemy;

                }
                else
                {
                    return Vector2Int.zero;
                }

            
        }



        protected override List<Vector2Int> SelectTargets()
        {
            ///////////////////////////////////////
            // Homework 1.4 (1st block, 4rd module)
            ///////////////////////////////////////
            List<Vector2Int> result = GetAllTargets().ToList();
            


            Vector2Int closestEnemyCoordinates = Vector2Int.zero;
            float enemy = float.MaxValue;

            Vector2Int enemyBase = runtimeModel.RoMap.Bases[IsPlayerUnitBrain? RuntimeModel.PlayerId : RuntimeModel.BotPlayerId];


            if (result.Count() != 0)
            {
                foreach (var i in result)
                {
                    var coordinatesToBase = DistanceToOwnBase(i);
                    if (coordinatesToBase < enemy)
                    {
                        enemy = coordinatesToBase;
                        closestEnemyCoordinates = i;

                    }
                    
                }
                if (IsTargetInRange(closestEnemyCoordinates))
                {
                    
                    result.Add(closestEnemyCoordinates);
                    result.Clear();
                    return result;
                }
                else
                {
                    _vector2s.Clear();
                    _vector2s.Add(closestEnemyCoordinates);
                    
                    return _vector2s;
                }

            }
            else 
            {
                _vector2s.Clear();
                _vector2s.Add(enemyBase);
                return _vector2s;
            }

            


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