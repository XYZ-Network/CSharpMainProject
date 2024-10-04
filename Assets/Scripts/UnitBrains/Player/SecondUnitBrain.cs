using System.Collections.Generic;
using Model.Runtime.Projectiles;
using UnityEngine;
using System.Linq;
using static UnityEngine.GraphicsBuffer;
using Model;
using UnityEditor;
using Utilities;
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
        private List<Vector2Int> TargetOutOfRange = new List<Vector2Int>();
        private static int counter = 0;
        private int unitNumber;
        private static int maxTargets = 3;
        
        public SecondUnitBrain()
        {
            UnitNumber = Counter;
            Debug.Log($"New unit number {Counter}");
            Counter++;
        }
        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            ///////////////////////////////////////
            // Homework 1.3 (1st block, 3rd module)
            ///////////////////////////////////////
            _temperature = GetTemperature();
            if (_temperature < overheatTemperature)
            {
                IncreaseTemperature();
                for (float i = 0; i < _temperature; i++)
                {
                    var projectile = CreateProjectile(forTarget);
                    AddProjectileToList(projectile, intoList);
                }
            }
            ///////////////////////////////////////
        }

        public override Vector2Int GetNextStep()
        {
            Vector2Int position = unit.Pos;
            if (TargetOutOfRange.Any())
            {
                Debug.Log("Цели вне досягаемоссти " + TargetOutOfRange.Any());
                foreach (var target in TargetOutOfRange)
                {
                    position = unit.Pos;
                    position = position.CalcNextStepTowards(target);
                }
            }
            return position;
        }

        protected override List<Vector2Int> SelectTargets()
        {
            ///////////////////////////////////////
            // Homework 1.4 (1st block, 4rd module)
            ///////////////////////////////////////
            IEnumerable<Vector2Int> resultAsIE = GetAllTargets();
            List<Vector2Int> result;
            Vector2Int enemy = new Vector2Int(0, 0);
            Vector2Int enemyBase = runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId];

            result = resultAsIE.ToList();

            if (!result.Any())
                result.Add(enemyBase);

            SortByDistanceToOwnBase(result);

            if (result.Count() > MaxTargets)
            {
                if (UnitNumber > MaxTargets)
                    enemy = result[MaxTargets];
                else
                    enemy = result[UnitNumber - 1];
            }
            else
            {
                if (UnitNumber > result.Count())
                    enemy = result[result.Count()];
                else
                    enemy = result[UnitNumber - 1];
            }
            
            result.Clear();

            if (IsTargetInRange(enemy))
                result.Add(enemy);
            else
                TargetOutOfRange.Add(enemy);

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

        public static int Counter { get; private set; }
        public int UnitNumber { get; private set; }
        public static int MaxTargets { get; private set; }
    }
}