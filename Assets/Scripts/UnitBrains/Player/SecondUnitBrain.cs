using System.Collections.Generic;
using GluonGui.Dialog;
using Model.Runtime.Projectiles;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using System.Linq;
using System;
using Model;
using UnityEditor.Experimental.GraphView;
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
        List<Vector2Int> inaccessibleTarget = new List<Vector2Int>();
        Vector2Int targetFromAllEnemy = new Vector2Int();
        List<Vector2Int> result = new List<Vector2Int>();

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;

            //Debug.Log("температура = " + GetTemperature());
            //Debug.Log("Перегрев = " + _overheated);

            if (GetTemperature() >= overheatTemperature)
            {
                return;
            }

            for (float i = _temperature + 1; i <= overheatTemperature && i > 0; i--)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
                //Debug.Log("Сгенерирован снаряд");
            }

            IncreaseTemperature();

        }

        public override Vector2Int GetNextStep()
        {
            if (!inaccessibleTarget.Contains(targetFromAllEnemy) && result.Contains(targetFromAllEnemy))
            {
                return unit.Pos;
            }

            else
            {
                Vector2Int position = unit.Pos;
                Vector2Int nextPosition = targetFromAllEnemy;
                position = position.CalcNextStepTowards(targetFromAllEnemy);
                return position;
            }
        }


        protected override List<Vector2Int> SelectTargets()
        {
            IEnumerable<Vector2Int> allTargets = GetAllTargets();
            List<Vector2Int> result = GetReachableTargets();

            if (allTargets.Count() == 0)
            {
                return new List<Vector2Int>(); ;
            }

            float minAllDistance = float.MaxValue;

            foreach (Vector2Int enemy in allTargets)
            {
                float distance = DistanceToOwnBase(enemy);
                if (distance < minAllDistance)
                {
                    minAllDistance = distance;
                    targetFromAllEnemy = enemy;
                }
            }

            if (!result.Contains(targetFromAllEnemy))
            {
                inaccessibleTarget.Add(targetFromAllEnemy);
            }

            if (result.Contains(targetFromAllEnemy))
            {
                result.Clear();
                result.Add(targetFromAllEnemy);

                while (result.Count > 1)
                {
                    result.RemoveAt(result.Count - 1);
                }
                return result;
            }
            else if (result.Count == 0 && inaccessibleTarget.Count == 0)
            {

                targetFromAllEnemy = runtimeModel.RoMap.Bases[
                  IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId];

                result.Clear();
                result.Add(targetFromAllEnemy);

                return result;
            }
            else
            {
                return new List<Vector2Int>();
            }
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
