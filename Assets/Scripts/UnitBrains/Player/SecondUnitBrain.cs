using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

using Model;
using Model.Runtime.Projectiles;

using Unity.VisualScripting.FullSerializer;

using UnityEngine;

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

        //!1a. Создай статическое поле, равное 0 для выдачи номеров - это счетчик.
        static int counter = -1;
        //!1b.Создай поле с номером юнита.
        int UnitID = counter++;

        //!1c. Создай константу - поле, по которому будет рассматриваться максимум целей для умного выбора. Присвой полю значение 3.
        int MaxUnitsInAttack = 3;
        //2b. Записываем самую опасную цель в эту коллекцию. Если цель в зоне досягаемости, то добавляем в result.
        //2a.Создаем новое поле для хранения целей, к которым нужно идти, но которые вне зоны досягаемости.
        public List<Vector2Int> NoReachableTargets = new List<Vector2Int>();
        public Vector2Int MostDangerTarget = new Vector2Int();
        protected override void GenerateProjectiles ( Vector2Int forTarget, List<BaseProjectile> intoList )
            {


            IncreaseTemperature();

            for (int i = 0; i < GetTemperature() && GetTemperature() < OverheatTemperature; i++)
                {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);

                }
            }

        public override Vector2Int GetNextStep ()
            {
            if (SelectTargets().Count != 0) { return unit.Pos; }
            else { return unit.Pos.CalcNextStepTowards(MostDangerTarget); }

            }



        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        protected override List<Vector2Int> SelectTargets ()
            {
            int NumberOffTargetAtack = new int();
            //1. Вместо достижимых целей получи все с помощью метода GetAllTargets().
            //var AllTargets = GetAllTargets();
            List<Vector2Int> AllTargets = new List<Vector2Int>();
            counter = 0;
            List<Vector2Int> result = new List<Vector2Int>();
            result.Clear();
            foreach (var target in GetAllTargets())
                {
                AllTargets.Add(target);
                }
            //!2c. Производим сортировку целей по дистанции. Для этого вызовем метод сортировки
            SortByDistanceToOwnBase(AllTargets);

            MostDangerTarget = AllTargets[0];
            ////!2d. Рассчитаем номер текущего юнита и определим, цель под каким номером следует бить.
            //NumberOffTargetAtack = UnitID;
            //while(NumberOffTargetAtack > MaxUnitsInAttack){NumberOffTargetAtack-=MaxUnitsInAttack;}
            //if (NumberOffTargetAtack >= AllTargets.Count) { NumberOffTargetAtack = AllTargets.Count-1; }

            result.Clear();
            if (IsTargetInRange(MostDangerTarget)) { result.Add(MostDangerTarget); }
            return result;
            }
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        public override void Update ( float deltaTime, float time )
            {
            Debug.Log($"Мой айди-{UnitID}");
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

        private int GetTemperature ()
            {
            if (_overheated) return (int)OverheatTemperature;
            else return (int)_temperature;
            }

        private void IncreaseTemperature ()
            {
            _temperature += 1f;
            if (_temperature >= OverheatTemperature) _overheated = true;
            }

        }
    }