using Model;
using Model.Runtime.Projectiles;
using System.Collections.Generic;
using System.Linq;
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
        private List<Vector2Int> getAll = new List<Vector2Int>();

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {


            if (GetTemperature() >= OverheatTemperature)
            {
                return;
            }

            IncreaseTemperature();

            int countBullet = GetTemperature();

            for (int i = 0; i < countBullet; i++)
            {
                BaseProjectile projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }



        }

        //getAll.Last();
        public override Vector2Int GetNextStep()
        {
            if(getAll.Count > 0)
            {
                Vector2Int position = unit.Pos;
                Vector2Int OneTarget = getAll.Last();
                position = position.CalcNextStepTowards(OneTarget);
                return position;
            }

            else {
                return unit.Pos;
            }
                
        }

        protected override List<Vector2Int> SelectTargets()
        {

            List<Vector2Int> result = GetAllTargets().ToList();
            getAll.Clear();
            if (result.Count == 0) 
            {
                if (IsPlayerUnitBrain == true)
                {
                    int id = RuntimeModel.BotPlayerId;
                    Vector2Int baseV = runtimeModel.RoMap.Bases[id];
                    result.Add(baseV);
                }
                else
                {
                    int id = RuntimeModel.PlayerId;
                    Vector2Int baseV = runtimeModel.RoMap.Bases[id];
                    result.Add(baseV);
                }

                return result;
            }

            

            float min = DistanceToOwnBase(result[0]);
            Vector2Int vector = result[0];

            if (IsTargetInRange(result[0]) == false)
            {
                getAll.Add(result[0]);
            }

            for (int i = 1; i < result.Count; i++)
            {
                if (IsTargetInRange(result[i]) == false)
                {
                    getAll.Add(result[i]);
                }

                if (min > DistanceToOwnBase(result[i]))
                {
                    vector = result[i];
                    min = DistanceToOwnBase(result[i]);
                }



            }

            getAll.Add(vector);

            result.Clear();

            if (IsTargetInRange(vector) == true)
            {
                result.Add(vector);
            }

            //else
            //{
            //    if (IsPlayerUnitBrain == true)
            //    {
            //        int id = RuntimeModel.BotPlayerId;
            //        Vector2Int baseV = runtimeModel.RoMap.Bases[id];
            //        result.Add(baseV);
            //    }
            //    else
            //    {
            //        int id = RuntimeModel.PlayerId;
            //        Vector2Int baseV = runtimeModel.RoMap.Bases[id];
            //        result.Add(baseV);
            //    }
            //}




                return result;




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
            if (_overheated == true)
                return (int)OverheatTemperature;
            else
                return (int)_temperature;
        }

        private void IncreaseTemperature()
        {
            _temperature += 1f;
            if (_temperature >= OverheatTemperature)
                _overheated = true;
        }
    }
}