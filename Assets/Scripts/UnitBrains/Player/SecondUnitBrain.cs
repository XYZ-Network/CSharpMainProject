using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Codice.Client.BaseCommands;
using Codice.CM.WorkspaceServer.Tree.GameUI.HeadTree;
using Model;
using Model.Runtime.Projectiles;
using PlasticGui.WorkspaceWindow.Diff;
using UnitBrains.Pathfinding;
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

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            ///////////////////////////////////////
            // Homework 1.3 (1st block, 3rd module)
        
            int temp = GetTemperature();
            
            if (temp >= overheatTemperature) 
            {
                return;
            }
 
            IncreaseTemperature();
            int CurrentTemp = GetTemperature();
            for (int i = 0; i <= CurrentTemp; i++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }
        }
                ///////////////////////////////////////
        public override Vector2Int GetNextStep()
            
        {
            if (SelectTargets().Any())
            {
                return unit.Pos;
            }
 
            
            if (targetsOutOfRange.Count > 0)
            {
              
                return unit.Pos.CalcNextStepTowards(targetsOutOfRange[0]);
            }
            return unit.Pos;
        }    

        protected override List<Vector2Int> SelectTargets()
        {
            ///////////////////////////////////////
            // Homework 1.4 (1st block, 4rd module)
            ///////////////////////////////////////
            List<Vector2Int> result = new List<Vector2Int>();
            targetsOutOfRange.Clear();

            List<Vector2Int> allTargets = GetAllTargets().ToList(); 
         

            if (allTargets.Count == 0)
            {
                int enemyId = IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.BotPlayerId;
                allTargets.Add(runtimeModel.RoMap.Bases[enemyId]);

            }

            float minDistance = float.MaxValue;
            Vector2Int nearestTarget = allTargets[0];

            foreach (var target in allTargets)

            {
                float distance = DistanceToOwnBase(target);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestTarget = target;
                }

            }
            if (IsTargetInRange(nearestTarget))
            {
                result.Add(nearestTarget);
            }
            else
            {
                targetsOutOfRange.Add(nearestTarget);
            }
            return result;
            ///////////////////////////////////////


        }
        private List<Vector2Int> targetsOutOfRange = new List<Vector2Int>();
        private List<Vector2Int> targetsInfRange = new List<Vector2Int>();
        

       
        
        
            
        

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