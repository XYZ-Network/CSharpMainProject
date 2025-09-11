using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using Model;
using UnitBrains;

//using System.Diagnostics;
using Model.Runtime.Projectiles;
using UnityEditor.Graphs;
using UnityEngine;
using Utilities;
using static UnityEditor.PlayerSettings;
using static UnityEngine.GraphicsBuffer;
using UnityEngine.UIElements;

namespace UnitBrains.Player
{
    public class SecondUnitBrain : DefaultPlayerUnitBrain
    {
        //public int NumberOfUnit;
        

        public const int MaxUnitAttack = 3;

        //public override string TargetUnitName => "Cobra Commando";
        public static string NameTargetUnit { get; set; } = "Cobra Commando"; 
        private string TargetUnitName = string.Empty;

        private const float OverheatTemperature = 3f;
        private const float OverheatCooldown = 2f;
        private float _temperature = 0f;
        private float _cooldownTime = 0f;
        private bool _overheated;

        public List<Model.Runtime.Unit> Groups;

        List<Model.Runtime.Unit> NewGroups = new();
        private List<Vector2Int> unreachebleTarget = new();

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            ///////////////////////////////////////
            // Homework 1.3 (1st block, 3rd module)
            ///////////////////////////////////////
            ///

            if (GetTemperature() < overheatTemperature)
            {
                for (int i = 0; i < _temperature + 1; i++)
                {
                    var projectile = CreateProjectile(forTarget);
                    AddProjectileToList(projectile, intoList);
                }
                IncreaseTemperature();
            }
        }
        ///////////////////////////////////////
        ///

        public override Vector2Int GetNextStep()
        {
            

            List<Vector2Int> unreachebleTarget = SelectTargets();
            List<Vector2Int> allTargets = SelectTargets();
            List<Vector2Int> result = SelectTargets();

            Vector2Int position = unit.Pos;
            Vector2Int Nextposition = Vector2Int.right + Vector2Int.up;

            //return base.GetNextStep();

            // Если нет достижимых целей вообще, идем к недостижимой цели
            if (result == null || result.Count == 0)
            {

                foreach (var target in unreachebleTarget)
                {
                    foreach (Model.Runtime.Unit unit in NewGroups)
                    {
                        return position.CalcNextStepTowards(target);
                    }

                }

            }
            // Если цели в рэнджэ атаки есть, остаемся на месте
            else if (result != null && result.Count > 0)
            {
                foreach (Model.Runtime.Unit unit in NewGroups)
                {
                    return unit.Pos;
                }

            }
            
            
            return position.CalcNextStepTowards(runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId]);
            
            


            //return position + Nextposition;


        }
        

        protected override List<Vector2Int> SelectTargets()
        {
            TargetUnitName = ProtectTargetName(NameTargetUnit);
            
            ///////////////////////////////////////
            // Homework 1.4 (1st block, 4rd module)
            ///////////////////////////////////////

            List<Vector2Int> allTargets =  GetAllTargets().ToList();
           /* allTargets.Clear();*/ /*очищаем с писок по заданию*/

            List<Vector2Int> result = new();


            unreachebleTarget.Clear();

            int enemyID = IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId;
            Vector2Int enemyBase = runtimeModel.RoMap.Bases[enemyID];

            var MinimumDistance = float.MaxValue;

           


            if (allTargets.Count > 1)
            {
                Vector2Int IntermediateVector = new Vector2Int { };

               
                
                SortByDistanceToOwnBase(allTargets);
                if (Model.Runtime.Unit.NumberOfUnit % 3 == 0)
                {
                    foreach (var mainTarget in allTargets)
                    {
                        if (IsTargetInRange(mainTarget))
                        {
                            result.Clear();
                            result.Add(mainTarget);
                        }
                        else
                        {

                            unreachebleTarget.Add(IntermediateVector);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < Model.Runtime.Unit.GroupUnits.Count; i += 3)
                    {
                        
                        List<Model.Runtime.Unit> Groups = new();
                        

                        for (int j = i; j < i + 3 && j < Model.Runtime.Unit.GroupUnits.Count; j++)
                        {
                            Groups.Add(Model.Runtime.Unit.GroupUnits[j]);
                            
                        }
                        NewGroups.AddRange(Groups);
                    }

                    foreach (var mainTarget in allTargets)
                    {
                        if (IsTargetInRange(mainTarget))
                        {
                            result.Clear();
                            result.Add(mainTarget);
                        }
                        else
                        {

                            unreachebleTarget.Add(IntermediateVector);
                        }
                    }

                }

            }
            else if (IsTargetInRange(enemyBase))
            {

                result.Add(enemyBase);

            }
            else
            {
                unreachebleTarget.Add(enemyBase);
            }
            return result;
            /////////////////////////////////////
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