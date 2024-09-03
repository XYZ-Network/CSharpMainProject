using System.Collections.Generic;
using Model.Runtime.Projectiles;
using Unity.VisualScripting;
using UnityEngine;
using Model;
using UnityEngine.UIElements;
using Utilities;
using GluonGui.Dialog;
using System.Linq;
using PlasticGui.WorkspaceWindow;
using Buffs;

namespace UnitBrains.Player
{
    public class SecondUnitBrain : DefaultPlayerUnitBrain
    {

        public override string TargetUnitName => "Cobra Commando";
        public float OverheatTemperature = 3f;
        private const float OverheatCooldown = 2f;
        private float _temperature = 0f;
        private float _cooldownTime = 0f;
        private bool _overheated;
        private List<Vector2Int> _notReachebleTarget = new List<Vector2Int>();
        private static int _unitCounter = 0;
        private int _unitNumber = _unitCounter++;
        private const int _maxUnits = 3;
        public int shootingCounter = 1;

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            if (GetTemperature() >= overheatTemperature)
            {
                return;
            }
            IncreaseTemperature();
            ///////////////////////////////////////
            // Homework 1.3 (1st block, 3rd module)
            ///////////////////////////////////////           
            ///

            for (int i = 0; i < GetTemperature(); i++)
            {
                Debug.Log(shootingCounter);
                for (int j = 0; j < shootingCounter; j++)
                {
                    var projectile = CreateProjectile(forTarget);
                    
                    AddProjectileToList(projectile, intoList);
                }
            }
            ///////////////////////////////////////
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