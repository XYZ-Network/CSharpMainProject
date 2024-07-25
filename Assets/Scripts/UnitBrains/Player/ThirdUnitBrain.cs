using System.Collections.Generic;
using Model;
using Model.Runtime.Projectiles;
using UnityEngine;

namespace UnitBrains.Player
{
    //Файл после проверки работоспособности, все отлично так же отрабатывает, как на видео
    public enum ThirdUnitState
    {
        Attack,
        Move
    }
    public class ThirdUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Ironclad Behemoth";
        private float _lastStateChangeTime;
        private float _delayBetweenModes = 1f;
        private const float OverheatTemperature = 3f;
        private const float OverheatCooldown = 2f;
        private float _temperature = 0f;
        private float _cooldownTime = 0f;
        private bool _overheated;
        private ThirdUnitState _state;
        private List<Vector2Int> TargetsOutOfRange = new List<Vector2Int>();
        private float _lastActionTime;


        public static int UnitID = 0;
        public int UnitNumber = UnitID++;
        public int MaximumTargets = 3;
        
        private readonly List<Vector2Int> _currentTarget = new();
        
        public ThirdUnitBrain()
        {
            _lastStateChangeTime = Time.time - _delayBetweenModes;
        }
        
        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            int currentTemperature = GetTemperature();
            
            if (currentTemperature < overheatTemperature)
            {
                for (int i = 0; i < currentTemperature; i++)
                {
                    var projectile = CreateProjectile(forTarget);
                    AddProjectileToList(projectile, intoList);
                    Debug.Log(_temperature + 1);
                }
                IncreaseTemperature();
                
            }
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
                int enemyBaseId = IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId;
                    Vector2Int enemyBase = runtimeModel.RoMap.Bases[enemyBaseId];
                    TargetsOutOfRange.Add(enemyBase);
                }
            else
            {
                SortByDistanceToOwnBase(TargetsOutOfRange);
                int TargetIndex = UnitNumber % MaximumTargets;
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

        public override void Update(float deltaTime, float time)
        {
            base.Update(deltaTime, time);
            ChangeMode();
        }

        private void ChangeMode()
        {
            if (Time.time - _lastStateChangeTime >= _delayBetweenModes)
            {
                _lastStateChangeTime = Time.time;

                if (_state == ThirdUnitState.Attack)
                {
                    _state = ThirdUnitState.Move;
                    GetNextStep();
                }
                else if (_state == ThirdUnitState.Move)
                {
                    _state = ThirdUnitState.Attack;
                    SelectTargets();
                    
                    if (_overheated)
                    {
                        _overheated = false;
                        _temperature = 0f;
                    }
                }
            }
        }
    }
}