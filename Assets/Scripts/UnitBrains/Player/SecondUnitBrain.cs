using System.Collections.Generic;
using System.Linq;
using Model;
using Model.Runtime.Projectiles;
using UnityEngine;
using Utilities;

namespace UnitBrains.Player
{
    public class SecondUnitBrain : DefaultPlayerUnitBrain
    {
        static int idCounter = 0;
        static readonly int maxTargetsToSelect = 3;

        public override string TargetUnitName => "Cobra Commando";
        private const float OverheatTemperature = 3f;
        private const float OverheatCooldown = 2f;
        private float _temperature = 0f;
        private float _cooldownTime = 0f;
        private bool _overheated;
        private List<Vector2Int> targetsOutOfRange = new();
        private int unitId;

        public SecondUnitBrain()
        {
            this.unitId = SecondUnitBrain.idCounter;
            SecondUnitBrain.idCounter++;
        }

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            ///////////////////////////////////////
            // Homework 1.3 (1st block, 3rd module)
            ///////////////////////////////////////        

            // Implement Temperature Overheating
            int curentTemperature = GetTemperature();
            if (curentTemperature >= overheatTemperature)
            {
                return;
            }
            IncreaseTemperature();

            // Implement Projectile Boost
            for (int projectileBoostIndex = 0; projectileBoostIndex <= curentTemperature; projectileBoostIndex++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }

            ///////////////////////////////////////
        }

        /// <summary>
        /// Вычисляет следующую позицию для перемещения юнита. Если выбранный враг находится в зоне 
        /// досягаемости для атаки или если нет доступных целей, возвращает текущую позицию юнита 
        /// (юнит остается на месте)
        /// </summary>
        /// <returns>Vector2Int координаты следующей позиции юнита. Если цель в зоне атаки или целей нет, возвращает текущую позицию</returns>
        public override Vector2Int GetNextStep()
        {
            if (this.targetsOutOfRange.Count > 0)
            {
                Vector2Int target = GetPersonalTarget();
                if (IsTargetInRange(target))
                {
                    return unit.Pos;
                }
                Vector2Int currentPosition = unit.Pos;
                return currentPosition.CalcNextStepTowards(target);
            }
            return unit.Pos;
        }

        /// <summary>
        /// Вычисляет персональную цель для юнита
        /// </summary>
        /// <returns>Vector2Int координаты персональной цели</returns>
        private Vector2Int GetPersonalTarget()
        {
            Vector2Int target;
            if (this.unitId < this.targetsOutOfRange.Count())
            {
                target = this.targetsOutOfRange[this.unitId];
            }
            else
            {
                target = this.targetsOutOfRange[0];
            }
            return target;
        }

        protected override List<Vector2Int> SelectTargets()


        {
            ///////////////////////////////////////
            // Homework 1.4 (1st block, 4rd module)
            ///////////////////////////////////////
            List<Vector2Int> result = new();
            Vector2Int enemyBase = runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId];
            IEnumerable<Vector2Int> allTargets = GetAllTargets();
            List<Vector2Int> targets = removeEnemyBaseAndConvertToList(allTargets);

            List<Vector2Int> removeEnemyBaseAndConvertToList(IEnumerable<Vector2Int> allTargets)
            {
                List<Vector2Int> targets = new();
                foreach (Vector2Int target in allTargets)
                {
                    if (target != enemyBase)
                    {
                        targets.Add(target);
                    }
                }
                return targets;
            }

            this.targetsOutOfRange.Clear();
            if (targets.Count > 0)
            {

                SortByDistanceToOwnBase(targets);
                int index = 0;
                while (index < SecondUnitBrain.maxTargetsToSelect)
                {
                    if (targets.Count() > index)
                    {
                        this.targetsOutOfRange.Add(targets[index]);
                    }
                    index++;
                }
                Vector2Int t = GetPersonalTarget();
                if (IsTargetInRange(t))
                {
                    result.Add(t);
                }
            }
            else
            {
                this.targetsOutOfRange.Add(enemyBase);
                if (IsTargetInRange(enemyBase))
                {
                    result.Add(enemyBase);
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