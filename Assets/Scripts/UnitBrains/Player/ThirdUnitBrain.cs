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
using static System.TimeZoneInfo;
using static UnityEngine.GraphicsBuffer;

namespace UnitBrains.Player
{
    public class ThirdUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Ironclad Behemoth";
        private enum UnitMode { Moving, Attacking }
        public List<Vector2Int> _targetNoRangeAttack = new();
        private UnitMode _mode = UnitMode.Moving;
        private bool _isTransitioning = false;
        private float _transitionTimer = 0f;
        private const float TransformTime = 1f;
        private Vector2Int _closestTarget;

        public override Vector2Int GetNextStep()
        {
            
            if (_isTransitioning || _mode != UnitMode.Moving)// Не едем во время перехода и вне режима движения
                return unit.Pos;

            if (_targetNoRangeAttack.Count == 0)// Нет цели для движения стоим
                return unit.Pos;

            Vector2Int target = _targetNoRangeAttack[0];

            if (IsTargetInRange(target))// Цель уже в зоне атаки останавливаемся
                return unit.Pos;

            return unit.Pos.CalcNextStepTowards(target);// Делаем шаг к цели
        }

        protected override List<Vector2Int> SelectTargets()
        {
            List<Vector2Int> result = new();

            if (_isTransitioning || _mode != UnitMode.Attacking)// Не стреляем во время перехода и вне режима атаки
                return result;

            if (IsTargetInRange(_closestTarget))
                result.Add(_closestTarget);

            return result;
        }

        public override void Update(float deltaTime, float time)
        {
            base.Update(deltaTime, time);

            _closestTarget = GetClosestTarget();
            bool inRange = IsTargetInRange(_closestTarget);

            _targetNoRangeAttack.Clear();
            if (!inRange)
                _targetNoRangeAttack.Add(_closestTarget);

            UnitMode desiredMode = inRange ? UnitMode.Attacking : UnitMode.Moving;

            if (_isTransitioning)
            {
                _transitionTimer -= deltaTime;
                if (_transitionTimer <= 0f)
                {
                    _isTransitioning = false;
                    _mode = desiredMode;
                    _transitionTimer = 0f;
                }
                return;
            }

            if (_mode != desiredMode)
            {
                _isTransitioning = true;
                _transitionTimer = TransformTime;
            }
        }

        private Vector2Int GetClosestTarget()
        {
            List<Vector2Int> allTargets = GetAllTargets().ToList();
            
            if (allTargets.Count == 0)
            {
                Vector2Int enemyBase = runtimeModel.RoMap.Bases[
                    IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId];// Если вражеских юнитов нет движемся к базе
                allTargets.Add(enemyBase);
            }

            SortByDistanceToOwnBase(allTargets);
            return allTargets[0];
        }
    }
}