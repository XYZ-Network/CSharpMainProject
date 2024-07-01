using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using GluonGui.Dialog;
using Model;
using Model.Runtime.Projectiles;
using UnitBrains.Pathfinding;
using UnityEditor.Experimental.GraphView;
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
        List<Vector2Int> targets = new List<Vector2Int>();
        private static int unitCounter = 0;
        private int unitNumber = 0;
        private const int maxTargets = 3;


        public SecondUnitBrain()
        {
            unitNumber = unitCounter++;
        }

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            float currentTemperature = GetTemperature();
            if (currentTemperature >= overheatTemperature)
            {
                return;
            }

            IncreaseTemperature();
            for (int i = 0; i <= currentTemperature; i++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }
        }

        public override Vector2Int GetNextStep()
        {
            if (targets.Count > 0)
            {
                if (IsTargetInRange(targets[0]))
                {
                    return unit.Pos;
                }
                var path = new AStarUnitPath(runtimeModel, unit.Pos, targets[0]);
                return path.GetNextStepFrom(unit.Pos);
            }
            else
            {
                return unit.Pos;
            }
            //return base.GetNextStep();
        }


        protected override List<Vector2Int> SelectTargets()
        {
            List<Vector2Int> result = new List<Vector2Int>();

            targets.Clear();

            foreach (Vector2Int target in GetAllTargets())
            {
                targets.Add(target);
            }
            if (targets.Count == 0)
            {
                if (IsPlayerUnitBrain)
                {
                    targets.Add(runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId]);
                }
                else
                {
                    targets.Add(runtimeModel.RoMap.Bases[RuntimeModel.PlayerId]);
                }
            }
            else
            {
                targets.Sort((x, y) => DistanceToOwnBase(x).CompareTo(DistanceToOwnBase(y)));

                for (int i = 0; i < maxTargets && i < targets.Count; i++)
                {
                    int targetIndex = (unitNumber + i) % targets.Count;

                    if (IsTargetInRange(targets[targetIndex]))
                    {
                        result.Add(targets[targetIndex]);
                    }
                }
            }
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