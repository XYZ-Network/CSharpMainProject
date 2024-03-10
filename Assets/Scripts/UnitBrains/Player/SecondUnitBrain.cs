using System;
using System.Collections.Generic;
using System.Linq;
using Model;
using Model.Runtime.Projectiles;
using UnityEngine;
using UnityEngine.UIElements;
// Start HomeWork-7
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

        // ДЗ-7
        private const int MaxTargetsForAttack = 4; // максимально число юнитов для атаки 
        private static int _playerIdCount = 0; // счётчик ID
        private int _playerUnitId = _playerIdCount++; // нам не нужно создавать конструктор, можно просто внести в переменную // ??? // почему именно такая реализация???
        private List<Vector2Int> _targetsToMove = new List<Vector2Int>(); // поле для всех опасных целей // ДЗ-6


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
            }
        }

        public override Vector2Int GetNextStep()
        {
            /*Vector2Int currentTarget = Vector2Int.zero; // завел вспомогательную переменную в которой будут координаты самой опасной цели на данный момент, по умолчанию будет x=0, y=0

            if (_targetsToMove.Count > 0)
            {
                currentTarget = _targetsToMove[0]; // если список _allCurrentTargets не пуст, присвой координаты цели со списка _allCurrentTargets под индексом 0 (так как он там единственный)
            }
            else
            {
                currentTarget = unit.Pos; // если список пуст, то присвой координаты самого себя, стой на месте, совершай выстрелы на расстоянии.
            }

            if (IsTargetInRange(currentTarget)) // если цель в зоне досягаемости, то стой на месте. вернёт позицию самого юнита. Атака на расстоянии.
            {
                return unit.Pos; // позиция самого себя(юнита)
            }
            else
            {
                return CalcNextStepTowards(currentTarget); // в противном случае верни позицию самого опасного врага к которому нужно ехать
            }*/
            return base.GetNextStep();

        }


        protected override List<Vector2Int> SelectTargets()
        {
            List<Vector2Int> _allTargets = new List<Vector2Int>(); // список всех целей
            List<Vector2Int> _targetsForAttack = new List<Vector2Int>(); // список целей для атаки

            int indexCurrentTargetForAttack = _playerUnitId % MaxTargetsForAttack; // расчёт индекса, почему такая формула? 

            Vector2Int closestTarget = new(); // координаты ближайший цели

            int minWeight = int.MaxValue; // расскажи про вес?

            foreach (var target in GetAllTargets()) // перебор всех целей, GetAllTargets() получает все цели
            {
                _allTargets.Add(target); // заполняем список всеми целями
            }

            SortByDistanceToOwnBase(_allTargets); // сортируем по возрастанию цели

            for (int i = 0; i < _allTargets.Count; i++) // перебираем все цели
            {
                int weight = Math.Abs(i - indexCurrentTargetForAttack); // Math.Abs - это модуль

                if (IsTargetInRange(_allTargets[i]))
                {
                    if (weight < minWeight)
                    {
                        minWeight = weight;
                        closestTarget = _allTargets[i];
                        _targetsForAttack.Add(closestTarget);
                    }
                }

                else
                {
                    _targetsToMove.Add(_allTargets[i]);
                }
            }


            if (_allTargets.Count == 1)
            {
                var enemyBaseTarget = runtimeModel.RoMap.Bases[
                IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId];
                _targetsForAttack.Add(enemyBaseTarget);
            }

            return _targetsForAttack;

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