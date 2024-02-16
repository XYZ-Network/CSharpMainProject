using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Model;
using Model.Runtime.Projectiles;
using UnityEngine;
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

        private List<Vector2Int> _allCurrentTargets = new List<Vector2Int>(); // поле для всех опасных целей // ДЗ6

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
            Vector2Int currentTarget = Vector2Int.zero; // завел вспомогательную переменную в которой будут координаты самой опасной цели на данный момент, по умолчанию будет x=0, y=0

            if (_allCurrentTargets.Count > 0)
            {
                currentTarget = _allCurrentTargets[0]; // если список _allCurrentTargets не пуст, присвой координаты цели со списка _allCurrentTargets под индексом 0 (так как он там единственный)
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
            }
        }


        protected override List<Vector2Int> SelectTargets()
        {

            List<Vector2Int> mostDangerousTarget = new List<Vector2Int>();   // список с самым опасным Врагом

            Vector2Int mostDangerousTargetPosition = Vector2Int.zero; // позиция самого опасного врага

            float enemyWithMinDistanceToBaseValue = float.MaxValue; // промежуточная переменная для хранения минимального расстояния от Врага к Базе

            foreach (Vector2Int dangerTarget in GetAllTargets()) // перебираем в цикле все вражеские цели на карте, метод GetAllTargets() получит все цели.
            {

                float enemyDistanceToBaseValue = DistanceToOwnBase(dangerTarget); // промежуточная переменная для врага в которой будет хранится абсолютное расстояние от Врага к Базе

                if (enemyDistanceToBaseValue < enemyWithMinDistanceToBaseValue) // условие, дистанция от нашей базы до врага меньше enemyWithMinDistanceToBaseValue
                {
                    enemyWithMinDistanceToBaseValue = enemyDistanceToBaseValue; // то, запишем эту дистанцию в enemyWithMinDistanceToBaseValue

                    mostDangerousTargetPosition = dangerTarget; // позиция самого опасного врага, запишем её в Vector2Int mostDangerousTargetPosition
                }
            }

            _allCurrentTargets.Clear(); // очистка верхнего списка, тот, что в шапке(в общей области видимости)

            if (enemyWithMinDistanceToBaseValue < float.MaxValue) // грубо говоря, если есть цель, то выполни условие
            {
                _allCurrentTargets.Add(mostDangerousTargetPosition); // самую опасную цель, добавим в список _allCurrentTargets, что вверху в общей видимости

                if (IsTargetInRange(mostDangerousTargetPosition)) // если самый опасный враг в области видимости, 
                {
                    mostDangerousTarget.Add(mostDangerousTargetPosition); // то добавим его в список mostDangerousTarget и вернём его для атаки
                }
            }
            else // если цели не найдены, то передай локацию Вражеской базы
            {
                if (IsPlayerUnitBrain)
                {
                    var enemyBaseTarget = runtimeModel.RoMap.Bases[
                IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId];
                    _allCurrentTargets.Add(enemyBaseTarget);
                }
            }

            return mostDangerousTarget;

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