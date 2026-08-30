using Model;
using Model.Runtime.Projectiles;
using PlasticPipe.PlasticProtocol.Messages;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities;
using static UnityEngine.GraphicsBuffer;
using UnitBrains.Pathfinding;

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

        private BaseUnitPath _activePath = null;
        public override BaseUnitPath ActivePath => _activePath;
        
        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            ///////////////////////////////////////
            // Homework 1.3 (1st block, 3rd module)
            ///////////////////////////////////////
            if (GetTemperature() >= overheatTemperature)
            {
                return;
            }
            else
            {
                IncreaseTemperature();
                for (int i = 0; i<_temperature; i++)   
                {
                    var projectile = CreateProjectile(forTarget);
                    AddProjectileToList(projectile, intoList);
                }
            }
        }

        List<Vector2Int> NextEnemyUnderDistance = new List<Vector2Int>();

        public override Vector2Int GetNextStep()
        {
            var priorityTarget = priorityActions.GetPriorityStep(unit);
            Vector2Int unitPos = unit.Pos;
            if (NextEnemyUnderDistance.Contains(priorityTarget))
            {
                var target = priorityTarget;
                _activePath = new SmartUnitPath(runtimeModel, unit.Pos, target);
                return _activePath.GetNextStepFrom(unitPos);
            }
            if (GetReachableTargets().Contains(NextEnemyUnderDistance[0]))
            {
                return unit.Pos;
            }
            return unitPos.CalcNextStepTowards(priorityTarget);
            //return base.GetNextStep();

        }

        List<int> EnemyIDs = new List<int>();
        private static int unitID = 0;
        private int maxEnemyCount = 3;

        protected override List<Vector2Int> SelectTargets()
        {
            ///////////////////////////////////////
            // Homework 1.4 (1st block, 4rd module)
            /*/
            Куча текста режeт глаза. 
            /*/

            List<Vector2Int> result = new List<Vector2Int>();

            Vector2Int enemyBase = runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId];
            Vector2Int mainTarget = new Vector2Int();

            IEnumerable<Vector2Int> possibleTarget = GetAllTargets();

            result.Clear();

            if (possibleTarget.Count() != 0)
            {
                foreach (var target in possibleTarget)
                {
                    result.Add(target);
                }
            }
            else
            {
                result.Add(enemyBase);
                return result;
            }

            SortByDistanceToOwnBase(result);

            foreach (var target in result)
            {
                unitID++;
                EnemyIDs.Add(unitID);
                int index;
                if (result.Count > maxEnemyCount)
                {
                    index = EnemyIDs.Count % maxEnemyCount;
                }
                else
                {
                    index = EnemyIDs.Count % result.Count;
                }
                mainTarget = result[index];                
            }
            result.Clear();

            if (GetReachableTargets().Contains(mainTarget))
            {
                result.Add(mainTarget);
            }
            else
            {
                NextEnemyUnderDistance.Clear();
                NextEnemyUnderDistance.Add(mainTarget);
            }

            return result;

            /*/
            Гы. Я учусь на XYZ!!!! 
            /*/
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