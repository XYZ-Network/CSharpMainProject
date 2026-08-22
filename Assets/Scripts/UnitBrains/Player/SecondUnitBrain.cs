using Codice.Client.Common.GameUI;
using GluonGui.Dialog;
using Model;
using Model.Runtime.Projectiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TMPro;
using UnityEditor;
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
        public List<Vector2Int> _targetNoRangeAttack = new();
        private static int targetCounter { get; set; } = 0;
        private int unitNumber { get; set; } = GetAndIncreaseUnitCounter();
        public static int MaxCounter = 3;
        private static int GetAndIncreaseUnitCounter()
        {
            return targetCounter++;
        }

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            ///////////////////////////////////////
            // Homework 1.3 (1st block, 3rd module)
            ///////////////////////////////////////

            if (GetTemperature() >= overheatTemperature)
                return;
            else
            {
                for (int a = 0; a <= GetTemperature(); ++a)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }
                IncreaseTemperature();
            }
        }

        ///////////////////////////////////////
        public override Vector2Int GetNextStep()
        {
            if (_targetNoRangeAttack.Count == 0)// Нет цели для движения — стоим
                return unit.Pos;

            Vector2Int target = _targetNoRangeAttack[0];

            if (IsTargetInRange(target))// Цель уже в зоне атаки — останавливаемся
                return unit.Pos;

            return unit.Pos.CalcNextStepTowards(target);// Делаем шаг к цели
        }

        protected override List<Vector2Int> SelectTargets()
        {
            List<Vector2Int> result = new();// Цели, которые юнит атакует в этом тике (в зоне досягаемости)

            List<Vector2Int> allTargets = GetAllTargets().ToList();// Все доступные цели

            if (allTargets.Count == 0)
            {
                Vector2Int enemyBase = runtimeModel.RoMap.Bases[
                    IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId];// Если вражеских юнитов нет — целимся в базу
                allTargets.Add(enemyBase);
            }

            SortByDistanceToOwnBase(allTargets);// Сортируем по расстоянию до нашей базы

            Vector2Int closestTarget = allTargets[0];// Самая близкая цель — наш приоритет

            _targetNoRangeAttack.Clear();// Очищаем список для движения на каждом тике,
            // чтобы туда не попадали «устаревшие» цели

            if (IsTargetInRange(closestTarget))
            {

                result.Add(closestTarget);// Цель в зоне атаки — бьём её, идти не нужно
            }
            else
            {

                _targetNoRangeAttack.Add(closestTarget);// Цель вне зоны — идём к ней, но пока не атакуем
            }

            return result;
        }

        ///////////////////////////////////////
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