using System.Collections.Generic;
using Model;
using Model.Runtime.Projectiles;
using PlasticGui.WorkspaceWindow;
using UnityEngine;
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

        
        //ДЗ N4
        private List<Vector2Int> _dangerousTargetOutOfRange = new List<Vector2Int>();

        //ДЗ N5
        private static int _counter = 0;
        private int UnitNumber;
        const int MAXnumberOfTargets = 3;

        public SecondUnitBrain() 
        {
            
            UnitNumber = _counter;
            _counter++;
            Debug.Log("Unit number " + UnitNumber);
        }

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            ///////////////////////////////////////
            // Homework 1.3 (1st block, 3rd module)

            int currentTemperature = GetTemperature();
            if (currentTemperature < overheatTemperature)
            {
                for(int shot = 0; shot <= currentTemperature; shot++)
                {
                    var projectile = CreateProjectile(forTarget);
                    AddProjectileToList(projectile, intoList);
                }
                IncreaseTemperature();
            }
            else return;
            

            ///////////////////////////////////////           
            
            ///////////////////////////////////////
        }

        public override Vector2Int GetNextStep()
        {
            
            if (_dangerousTargetOutOfRange.Count > 0) 
            {
                Vector2Int unitPosition = unit.Pos;
                Vector2Int target = _dangerousTargetOutOfRange[0];

                unitPosition = unitPosition.CalcNextStepTowards(target);
                return unitPosition;
            }
            return unit.Pos;

        }

        protected override List<Vector2Int> SelectTargets()
        {
            ///////////////////////////////////////
            // Homework 1.4 (1st block, 4th module)
            ///////////////////////////////////////

            _dangerousTargetOutOfRange.Clear();                      

            List<Vector2Int> result = new List<Vector2Int>();
            List<Vector2Int> resultList = new List<Vector2Int>();
            IEnumerable<Vector2Int> ListOfAllTargets = GetAllTargets();
            Vector2Int closestTarget = new Vector2Int();
            

            foreach (Vector2Int target in ListOfAllTargets)
            {
                resultList.Add(target);
            }

            if (resultList.Count <= 0)
            {
                var baseTarget = runtimeModel.RoMap.Bases[
                IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId];
                resultList.Add(baseTarget);
            }

            SortByDistanceToOwnBase(resultList);

            int targetIndex = UnitNumber;

            while (targetIndex > MAXnumberOfTargets) 
            {
                targetIndex -= MAXnumberOfTargets;            
            }

            if (targetIndex > resultList.Count)
            {
                closestTarget = resultList[0];
            }
            else
            {
                closestTarget = resultList[targetIndex - 1];
            }

            if (IsTargetInRange(closestTarget))
            {
                result.Add(closestTarget);
            }
            else
            {
                _dangerousTargetOutOfRange.Add(closestTarget);
            }

            resultList.Remove(closestTarget);

            if (resultList.Count <= 0) 
            {
                _counter = 1;
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