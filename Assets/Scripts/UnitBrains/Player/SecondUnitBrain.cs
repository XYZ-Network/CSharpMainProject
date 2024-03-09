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
        private static int _unitsCounter = 0;
        private const int _maxTargets = 4;
        public int UnitID {  get; private set; }
        List<Vector2Int> allTargets = new();

        public SecondUnitBrain()
        {
            UnitID = _unitsCounter++;
        }

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            float temp = GetTemperature();
            if (temp >= overheatTemperature)
            {
                return;
            }
            IncreaseTemperature();
            for (int i = 0; i <= temp; i++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
                Debug.Log("ВЫСТРЕЛ");
            }
        }

        public override Vector2Int GetNextStep()
        {
            Vector2Int target = allTargets.Count > 0 ? allTargets[0] : unit.Pos;

            return IsTargetInRange(target) ? unit.Pos : unit.Pos.CalcNextStepTowards(target);
        }

        protected override List<Vector2Int> SelectTargets() 
        {
            List<Vector2Int> result = new();
            Vector2Int enemyBase = runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId];
            allTargets.Clear();

            foreach (var target in GetAllTargets())
            {
                if (target != enemyBase)
                {
                    allTargets.Add(target);
                }
            }

            if (allTargets.Count == 0) 
            {
                allTargets.Add(enemyBase);
            }

            SortByDistanceToOwnBase(allTargets);

            int targetId = UnitID % _maxTargets;
            int targetNum = Mathf.Min(targetId, allTargets.Count - 1);
            Vector2Int betsTarget = allTargets[targetNum];

            if (IsTargetInRange(betsTarget))
            {
                result.Add(betsTarget);
            }

            return result; // Возвращаем результат. Он может быть либо пустым, либо с таргетом(не с базой) 
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