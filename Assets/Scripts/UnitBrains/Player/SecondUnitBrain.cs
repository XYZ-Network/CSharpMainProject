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
        public override string TargetUnitName => "Cobra Commando";
        private const float OverheatTemperature = 3f;
        private const float OverheatCooldown = 2f;
        private float _temperature = 0f;
        private float _cooldownTime = 0f;
        private bool _overheated;
        

        public static int iDMarker = 0;
        private int iDSelectUnit = iDMarker ++;
        private const int limitTargets = 3;

        public List<Vector2Int> unReachableTarget = new List<Vector2Int>();
        public List<Vector2Int> resultForShoot = new List<Vector2Int>();


       

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {

            /////////////////////////////////////////
            //// Homework 1.3 (1st block, 3rd module)
            /////////////////////////////////////////           
            //var projectile = CreateProjectile(forTarget);
            //AddProjectileToList(projectile, intoList);
            /////////////////////////////////////////


            if (!_overheated)
            {
                IncreaseTemperature();
                int t = GetTemperature();

                for (int i = 0; i < t; i++)
                {
                    var projectile = CreateProjectile(forTarget);
                    AddProjectileToList(projectile, intoList);
                }
            }


        }

        public override Vector2Int GetNextStep()
        {
           Vector2Int unitPos = unit.Pos;

            if (unReachableTarget.Count == 1)
            {
                Vector2Int currentTarget = unReachableTarget[0];
                if (!IsTargetInRange(currentTarget))
                {
                    return unitPos.CalcNextStepTowards(currentTarget);
                }

            }
            return base.GetNextStep();
        }

        protected override List<Vector2Int> SelectTargets()
        {

            resultForShoot.Clear();
            List<Vector2Int> result = GetAllTargets().ToList();

            if (result.Count == 1)
            {
                var baseEnemy = runtimeModel.RoMap.Bases[
                    IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId];

                if (IsTargetInRange(baseEnemy))
                {
                    return ListSender(resultForShoot, baseEnemy);
                }
                else
                {
                    UnreachedListSender(baseEnemy);
                }

            }
            else
            {
                SortByDistanceToOwnBase(result);
                int convertedID = IDConverter(iDSelectUnit, result); 
                for (int i = 0; i < result.Count && i < limitTargets ; i++)
                {
                    var target = result[i];
                    int targetID = i;

                    if (targetID == convertedID)
                    {
                        if (IsTargetInRange(target))
                        {
                            return ListSender(resultForShoot, target);
                        }
                        else
                        {
                            UnreachedListSender(target);
                        }
                    }
                }
            }
            return resultForShoot;
        }

        private int IDConverter(int id, List<Vector2Int> targetList) // if our Id is Higher than target number/ other IDs are dead/ anything else we should convert this ID
        {
            int counter = 0;
            if (targetList.Count <= 3)
            {
                counter = id / targetList.Count;
                id = id - counter * targetList.Count;
                
                return id;
            }
            else
            {
                if (id >= 3)
                {
                    
                    counter = id / 3;
                    id = id - counter * 3;
                    return id;
                }
            }
            return id;
            
        }
        private List<Vector2Int> ListSender(List<Vector2Int> shootList, Vector2Int target)
        {
            shootList.Clear();
            shootList.Add(target);
            return shootList;
        }
        
        private void UnreachedListSender(Vector2Int target)
        {
            unReachableTarget.Clear();
            unReachableTarget.Add(target);
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