using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.UnitBrains.Pathfinding;
using Model;
using Model.Runtime;
using Model.Runtime.Projectiles;
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

        private List<Vector2Int> unreachableTarget = new();
        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            

            if (GetTemperature() < overheatTemperature)
            {
                int shotsCount = unit.Config.EnebledBuff ? 2 : 1;

                for (int shot = 0; shot < shotsCount; shot++)
                {
                    for (int i = 0; i < _temperature + 1; i++)
                    {
                        var projectile = CreateProjectile(forTarget);
                        AddProjectileToList(projectile, intoList);
                    }
                    
                }
                IncreaseTemperature();
            }
        }
        public override Vector2Int GetNextStep()
        {
            int enemyID = IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId;
            Vector2Int enemyBase = runtimeModel.RoMap.Bases[enemyID];
           
            List<Vector2Int> allTargets = SelectTargets();
            List<Vector2Int> result = SelectTargets();
            var UnitGroups= SelectTargets();

            Vector2Int position = unit.Pos;
            Vector2Int Nextposition = Vector2Int.right + Vector2Int.up;
            if (result.Count != 0) //Если список достижимых целей не пуст стоим на месте
            {
                 //Debug.Log("Цель в радиусе атаки");
                 return unit.Pos;
            }
            else if (result.Count == 0 && unreachableTarget.Count != 0) //Если список достижимых целей пуст,а недостижымых не пуст,то идем к недостижимым 
            {
                 _activePath = new AStar(runtimeModel, unit.Pos, unreachableTarget[0]);
                 //Debug.Log("Целей в радиусе атаки нет,иду к ближайшей");
                 return _activePath.GetNextStepFrom(unit.Pos);
                
            }
            _activePath = new AStar(runtimeModel, unit.Pos, enemyBase);
            return position.CalcNextStepTowards(runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId]);
        }
        protected override List<Vector2Int> SelectTargets()
        {
            int enemyID = IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId;
            Vector2Int enemyBase = runtimeModel.RoMap.Bases[enemyID];
            
            List< Vector2Int > allTargets = GetAllTargets().ToList();
            List<Vector2Int> result = new();

            unreachableTarget.Clear();
            result.Clear();

            var UnitGroups = new List<List<Unit>>();
            var CopyUnitGroups = UnitGroups.ToList();

            if (allTargets.Count > 0) // проверка на пустой список
            {
                SortByDistanceToOwnBase(allTargets); // сортируем список по близости к базе игрока

                for (int i = 0; i < Unit.Group.Count; i+=3)
                {
                   var NewGroups = (Unit.Group).Skip(i).Take(3).ToList(); // группируем юнитов из списка Group, с шагом 3 элемента списка, в 3 разных группы
                   UnitGroups.Add(NewGroups);

                }
                for (int i = 0;i < UnitGroups.Count; i ++)
                {               
                        if (IsTargetInRange(allTargets[i]))
                        {
                            result.Add(allTargets[i]);              // каждой группе назначается своя цель
                    }
                        else
                        {
                            unreachableTarget.Add(allTargets[i]);
                        }                    
                }              
            }
            else 
            {
                unreachableTarget.Add(enemyBase); //в сулчае если сп сок пуст возварщаем по итогу координаты базы противника
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