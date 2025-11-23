using System.Collections.Generic;
using System.Linq;
using Model;
using Model.Runtime.Projectiles;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
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
        List<Vector2Int> Outreachable = new List<Vector2Int>();
        public int MaxPlayer = 0;

        private static int UnitCounter = 0;
        public int Counter { get; private set; }
        private const int MaxTargets = 3;


        public SecondUnitBrain()
        {
            Counter = UnitCounter++;
        }

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            if (GetTemperature() >= overheatTemperature)
            {
                return;
            }
            else
            {
                for (int i = 0; i <= GetTemperature(); i++)
                {
                    var projectile = CreateProjectile(forTarget);
                    AddProjectileToList(projectile, intoList);
                }
                IncreaseTemperature();

            }



        }

        public override Vector2Int GetNextStep()
        {
        return base.GetNextStep();
        Vector2Int target = Outreachable[0];
        if (Outreachable.Count > 0 && !IsTargetInRange(target))
           {
                return unit.Pos.CalcNextStepTowards(target);
           }

        else
            {
                return unit.Pos;
            }
        }

        protected override List<Vector2Int> SelectTargets()
        {
            List<Vector2Int> targets = new List<Vector2Int>();
            List<Vector2Int> result = new List<Vector2Int>();

            Outreachable.Clear();

            foreach (var target in GetAllTargets())
            {
                if (IsTargetInRange(target))
                {
                    targets.Add(target);

                }
                else
                {
                    Outreachable.Add(target);
                }
            }


            if (result.Count == 0 && Outreachable.Count == 0)
            {
                if (IsTargetInRange(runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId]))
                {
                    result.Add(runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId]);
                    return result;
                }
                Outreachable.Add(runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId]);
                return result;
            }

            SortByDistanceToOwnBase(targets);
            int targetID = Counter % MaxTargets;
            // Индекс таргета зависит от максимального кол-ва целей и айди нашего атакующего юнита 
            if (targetID < targets.Count)
            {
                result.Add(targets[targetID]);
            }
            else if (targets.Count > 0)
            {
                result.Add(targets[targets.Count - 1]); // последний элемент, ВАЖНО индекс положительный
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