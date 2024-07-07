using System.Collections.Generic;
using System.Linq;
using Model.Runtime.Projectiles;
using UnityEngine;
using Model;
using Utilities;
using TMPro;
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

        public static int unitCounter = 0;
        public int unitNamber = unitCounter++;
        public const int maxTarget = 3;

        List<Vector2Int> _targetOutOfRange = new List<Vector2Int>();
        
        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            float currentTemperature = GetTemperature();

            if (currentTemperature >= overheatTemperature)
            {
                return;
            }

            IncreaseTemperature();

            for (int i = 0; i < currentTemperature; i++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }
           
        }

        public override Vector2Int GetNextStep()
        {
            Vector2Int targetPosition = _targetOutOfRange.Count > 0 ? _targetOutOfRange[0] : unit.Pos;
            if (IsTargetInRange(targetPosition)) 
            {
                return unit.Pos;
            }
            else return unit.Pos.CalcNextStepTowards(targetPosition);
            
            
        }

        protected override List<Vector2Int> SelectTargets()
        {
            
            List<Vector2Int> result = new List<Vector2Int>();
            _targetOutOfRange.Clear();
                     
                        
            
            foreach (Vector2Int target in GetAllTargets()) 
            {
                    
                _targetOutOfRange.Add(target);

                if (_targetOutOfRange.Count == 0)
                {
                    _targetOutOfRange.Add(runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId]);
                }

                SortByDistanceToOwnBase(_targetOutOfRange);

                int currentUnitNamber = unitNamber % _targetOutOfRange.Count;

                int firstTargetNumber = Mathf.Min(currentUnitNamber, _targetOutOfRange.Count - 1);

                Vector2Int firstTarget = _targetOutOfRange[firstTargetNumber];


                if (IsTargetInRange(firstTarget))
                {
                    result.Add(firstTarget);
                    _targetOutOfRange.Clear();
                }

                return result;


            }

            return result;




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