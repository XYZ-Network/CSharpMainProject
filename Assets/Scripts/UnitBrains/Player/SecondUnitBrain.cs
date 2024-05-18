using System.Collections.Generic;
using Model.Runtime.Projectiles;
using UnityEngine;
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
        
        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            ///////////////////////////////////////
            // Homework 1.3 (1st block, 3rd module)
            ///////////////////////////////////////           
            float nowTemperature = GetTemperature();

            if (nowTemperature < overheatTemperature)
            {
                do
                {
            var projectile = CreateProjectile(forTarget);
            AddProjectileToList(projectile, intoList);

                    nowTemperature--;
                }
                while (nowTemperature >= 0);

                IncreaseTemperature();
            }
            ///////////////////////////////////////
        }

        public override Vector2Int GetNextStep()
        {
            return base.GetNextStep();
        }

        protected override List<Vector2Int> SelectTargets()
        {
            ///////////////////////////////////////
            // Homework 1.4 (1st block, 4rd module)
            ///////////////////////////////////////

            List<Vector2Int> result = GetReachableTargets();
            float minDistance = float.MaxValue;
            for (int i = 0; i < result.Count; i++)//If I write through Foreach, it gives me an error on lines 85-93 p.s. the error indicates that you cannot change their number while iterating over elements
            {
                float distanceToNowTarget = DistanceToOwnBase(result[i]);//I take this variable to calculate it once instead of repeated recalculations
                if (distanceToNowTarget < minDistance)
                {
                    minDistance = distanceToNowTarget;
                    result.Add(result[i]);
                }
                result.RemoveRange(0, result.Count - 1);
            }
            return result;

            //InvalidOperationException: Collection was modified; enumeration operation may not execute.
            //System.Collections.Generic.List`1 + Enumerator[T].MoveNextRare()(at < 834b2ded5dad441e8c7a4287897d63c7 >:0)
            //System.Collections.Generic.List`1 + Enumerator[T].MoveNext()(at < 834b2ded5dad441e8c7a4287897d63c7 >:0)
            //UnitBrains.Player.SecondUnitBrain.SelectTargets()(at Assets / Scripts / UnitBrains / Player / SecondUnitBrain.cs:55)
            //UnitBrains.BaseUnitBrain.GetProjectiles()(at Assets / Scripts / UnitBrains / BaseUnitBrain.cs:49)
            //Model.Runtime.Unit.Attack()(at Assets / Scripts / Model / Runtime / Unit.cs:65)
            //Model.Runtime.Unit.Update(System.Single deltaTime, System.Single time)(at Assets / Scripts / Model / Runtime / Unit.cs:57)
            //Controller.SimulationController.Update(System.Single deltaTime)(at Assets / Scripts / Controller / SimulationController.cs:33)
            //Utilities.TimeUtil.FixedUpdate()(at Assets / Scripts / Utilities / TimeUtil.cs:58)

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