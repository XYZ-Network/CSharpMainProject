using System.Collections.Generic;
using ActionGameFramework.Projectiles;
using Model.Runtime.Projectiles;
using Unity.VisualScripting;
using UnityEditor.SearchService;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

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
        
        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            ///////////////////////////////////////
            // Homework 1.3 (1st block, 3rd module)
            ///////////////////////////////////////    
            int a = GetTemperature();

            if (a >= overheatTemperature)
            {
                return;
            }

            for (int i = 0; i < a; i++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }

            IncreaseTemperature();
            ///////////////////////////////////////
            
        }

        public override Vector2Int GetNextStep()
        {
            foreach (var cel in _target)
            {
                if (_target.Count > 0)
                {
                    Vector2Int pos = _target[0];
                    pos = CalcNextStepTowards(pos);
                }
            }
            return pos;
        }
        
        protected override List<Vector2Int> SelectTargets()
        {
            ///////////////////////////////////////
            // Homework 1.4 (1st block, 4rd module)
            ///////////////////////////////////////
            List<Vector2Int> result = GetReachableTargets();
            List<Vector2Int> _targets = new List<Vector2Int>();
            
            float min = float.MaxValue;
            float max = float.MinValue;
            Vector2Int bestTarget = Vector2Int.zero;

            var alltarg = GetAllTargets();
            foreach ( var vector2 in alltarg )
            {
                var distance = DistanceToOwnBase(vector2);

                if (distance < min)
                {
                    min = distance;
                    bestTarget = vector2;
                }
                if (distance > max)
                {
                    max = distance;
                    bestTarget = vector2;
                }
            }

            _targets.Clear();
            if (min < float.MaxValue)&(max > float.MinValue)
            {
                _targets.Add(bestTarget);
            }
            if (min < float.MaxValue)&(max <  float.MinValue) 
            {
                result.Add(bestTarget);
            }
            return _target;
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