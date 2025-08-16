using System.Collections.Generic;
using Model;
using Model.Runtime.Projectiles;
using UnityEngine;
using UnityEngine.UIElements;
using Utilities;
using System.Linq;

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
        private List<Vector2Int> _pendingTargets = new List<Vector2Int>();//цели для движения

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            //a. Проверка перегрева оружия
            if (GetTemperature() >= overheatTemperature)
            {
                return;
            }
            
            //2.a Увеличение снарядов
            int MissileCount = GetTemperature(); // кол-во снарядов = текущей температуре
            for (int i = 0; i < MissileCount; i++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }
            //b. Вызов метода private void IncreaseTemperature()
            IncreaseTemperature();
        }

        public override Vector2Int GetNextStep()
        {
            Vector2Int position = unit.Pos;


            var reachableTargets = GetReachableTargets();
            if (reachableTargets.Count > 0)//Если в зоне атаки, атакуем
            {
                return position;
            }

            if (_pendingTargets != null && _pendingTargets.Count > 0)//Если врагов нет
            {
                Vector2Int nextPosition = _pendingTargets[0];
                Vector2Int step = position.CalcNextStepTowards(nextPosition);
                return step;
            }
            return position;
        }

        protected override List<Vector2Int> SelectTargets()

        {
            _pendingTargets.Clear();//очистка списка целей
            List<Vector2Int> allTarget = GetAllTargets().ToList();
            List<Vector2Int> result = new List<Vector2Int>();
            if (allTarget.Count > 0)
            {
                Vector2Int target = allTarget[0];// Выбираем ближайшую цель к базе
                float minDistance = DistanceToOwnBase(target);
                for (int i = 1; i < allTarget.Count; i++)
                {
                    float distance = DistanceToOwnBase(allTarget[i]);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        target = allTarget[i];
                    }
                }
                if (GetReachableTargets().Contains(target))// Проверка досягаемости
                    result.Add(target);
                else _pendingTargets.Add(target);
            }
            else
            {
                int enemyId = IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId;// если мы игрок → враг бот, // если мы бот → враг игрок
                Vector2Int enemyBase = runtimeModel.RoMap.Bases[enemyId];//координаты базы врага
                _pendingTargets.Add(enemyBase);//цели для движения
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