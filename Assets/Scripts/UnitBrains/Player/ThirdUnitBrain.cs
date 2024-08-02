using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml.Linq;
using Model;
using Model.Runtime;
using Model.Runtime.Projectiles;
using TMPro;
using UnitBrains.Player;
using UnityEngine;

namespace UnitBrains.Player
{
    public class ThirdUnitBrain : DefaultPlayerUnitBrain
    {
        public int UnitId { get; private set; }
        public override string TargetUnitName => "Ironclad Behemoth";
        private const float OverheatTemperature = 4f;
        private const float OverheatCooldown = 0f;
        private float _temperature = 0f;
        private float _cooldownTime = 0f;
        private bool _overheated;
        private List<Vector2Int> TargetsOutOfRange = new List<Vector2Int>();
        private static int UnitIndex = 0;
        private const int MaxTargets = 10;
        private bool walk = true;
        private bool shoot = false;
        public float _transitionTime = 1f;


        public ThirdUnitBrain()
        {
            UnitId = UnitIndex++;
            Debug.Log($"Unit number: {UnitId}");
        }
        
        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            if (shoot == true)
            {
                float overheatTemperature = OverheatTemperature;
                float temperatu = GetTemperature();
                if (temperatu >= overheatTemperature) return;
                for (float strel = 1f; strel <= temperatu; strel += 1f)
                {

                    var projectile = CreateProjectile(forTarget);
                    AddProjectileToList(projectile, intoList);
                }
            }
            IncreaseTemperature();
        }
        void Transition()
        {
            for (float Transition = 1f; Transition == _transitionTime; Transition++)
            {
                if (HasTargetsInRange())
                {
                    Shoot();
                }
                else
                {
                    Walk();

                }
            }
        }
        void Shoot()
        {          
                shoot = true;
                walk = false;          
        }
        void Walk()
        {         
                shoot = false;
                walk = true;           
        }
        protected override List<Vector2Int> SelectTargets()
        {
           
                List<Vector2Int> result = new List<Vector2Int>();
                Vector2Int TargetPosition;
                TargetsOutOfRange.Clear();

                foreach (Vector2Int target in GetAllTargets())
                {
                    TargetsOutOfRange.Add(target);
                }
                if (TargetsOutOfRange.Count == 0)
                {
                if (walk==true)
                {
                    int enemyBaseId = IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId;
                    Vector2Int enemyBase = runtimeModel.RoMap.Bases[enemyBaseId];
                    TargetsOutOfRange.Add(enemyBase);
                }
                }
            else
            {
                SortByDistanceToOwnBase(TargetsOutOfRange);
                int TargetIndex = UnitId % MaxTargets;
                if (TargetIndex > (TargetsOutOfRange.Count - 1))
                {
                    TargetPosition = TargetsOutOfRange[0];
                }
                else
                {
                    if (TargetIndex == 0)
                    {
                        TargetPosition = TargetsOutOfRange[TargetIndex];
                    }
                    else
                    {
                        TargetPosition = TargetsOutOfRange[TargetIndex - 1];
                    }

                }
                if (IsTargetInRange(TargetPosition))
                    result.Add(TargetPosition);
            }
            return result;
        }

        public override void Update(float deltaTime, float time)
        {
            Transition();

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
