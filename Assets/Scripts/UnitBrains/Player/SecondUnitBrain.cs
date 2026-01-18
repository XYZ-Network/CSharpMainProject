using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Codice.Client.BaseCommands.Differences;
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
        private static int Counter = 0;
        private int UnitNumber;
        private const int MaxTargets = 3;
        private List<Vector2Int> TargetList = new List<Vector2Int>();

        public SecondUnitBrain()
        {
            UnitNumber = Counter;
            Counter++;
        }

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            
            if (GetTemperature() < overheatTemperature)
            {
                IncreaseTemperature();
                for (int i = 0; i != GetTemperature(); i++)
                {
                    var projectile = CreateProjectile(forTarget);
                    AddProjectileToList(projectile, intoList);
                }
            }
        }

        public override Vector2Int GetNextStep()
        {
            Vector2Int targetPosition;
            targetPosition = TargetList.Count > 0 ? TargetList[0] : unit.Pos;
            return IsTargetInRange(targetPosition) ? unit.Pos : base.GetNextStep();
        }

        protected override List<Vector2Int> SelectTargets()
        {
            var iD = IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.BotPlayerId;
            var baseCoords = runtimeModel.RoMap.Bases[iD];

            TargetList.Clear();

            List<Vector2Int> result = GetAllTargets().ToList();
            List<Vector2Int> AllReachableTargets = GetReachableTargets();
            List<Vector2Int> closestTargets = new List<Vector2Int>();

            SortByDistanceToOwnBase(result);

            var closestTargetsCount = MaxTargets > AllReachableTargets.Count ? AllReachableTargets.Count : MaxTargets;
            closestTargets.AddRange(AllReachableTargets.GetRange(0, closestTargetsCount));

            var targetIndex = UnitNumber % MaxTargets;
            var indexExist = targetIndex < closestTargets.Count && targetIndex > 0;

            if (indexExist)
            {
                TargetList.Add(closestTargets[targetIndex]);
            }
            else if (closestTargets.Count > 0)
            {
                TargetList.Add(closestTargets[0]);
            }
            else
            {
                TargetList.Add(baseCoords);
            }

            return AllReachableTargets.Contains(TargetList.LastOrDefault()) ? TargetList : AllReachableTargets;
            //int TargetNumber = UnitNumber < MaxTargets ? UnitNumber : UnitNumber % MaxTargets;
            //Vector2Int Tmp;

            //if (result.Count == 0)
            //{
            //    TargetList.Clear();
            //    Vector2Int enemyBase = runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.PlayerId : RuntimeModel.BotPlayerId];
            //    if (IsTargetInRange(enemyBase))
            //        result.Add(enemyBase);
            //    else
            //        TargetList.Add(enemyBase);

            //}
            //else
            //{
            //    SortByDistanceToOwnBase(result);
            //    if (AllReachableTargets.Count == 0)
            //    {
            //        TargetList.Add((result.Count - 1) < TargetNumber ? result[0] : result[TargetNumber]);
            //        result.Clear();
            //    }
            //    else
            //    {
            //        Tmp = ((AllReachableTargets.Count - 1) < TargetNumber ? result[0] : result[TargetNumber]);
            //        result.Clear();
            //        result.Add(Tmp);
            //    }
            //}
            //return result;
            ///////////////////////////////////////
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
            if (_overheated) return (int)OverheatTemperature;
            else return (int)_temperature;
        }

        private void IncreaseTemperature()
        {
            _temperature += 1f;
            if (_temperature >= OverheatTemperature) _overheated = true;
        }
    }
}