using System;
using System.Collections;
using System.Collections.Generic;
using Model;
using Model.Runtime.Projectiles;
using Model.Runtime.ReadOnly;
using UnitBrains.Pathfinding;
using UnityEngine;
using Utilities;
using View;

namespace UnitBrains.Player
{
    public class FourthUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Buff Buddy";
        private const float OverheatTemperature = 3f;
        private const float OverheatCooldown = 2f;
        private float _temperature = 0f;
        private float _cooldownTime = 0f;
        private bool _overheated;

        private BaseUnitPath _activePath;
        private RuntimeModel _runtimeModel => ServiceLocator.Get<RuntimeModel>();


        // HW 13
        VFXView _vfxView => ServiceLocator.Get<VFXView>();
        TimeUtil _timeUtil => ServiceLocator.Get<TimeUtil>();
        private bool _isTargetBaseEnemy = true;
        private bool _isFrendlyUnitsInRange = false;
        private bool _isPaused = false;
        private bool _isActiveBuffUnit = false;
        Vector2Int _targetBasePosition = Vector2Int.zero;
        List<IReadOnlyUnit> _friendUnits = new List<IReadOnlyUnit>();
        //

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {               
            List<BaseProjectile> projecttilesForThisUnit = new List<BaseProjectile>(0);          
        }

        // HW 13
        public override Vector2Int GetNextStep()
        {
            if (!_isActiveBuffUnit) return unit.Pos;

            if (_isTargetBaseEnemy)
            {
                _targetBasePosition = runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId];
            } else
            {
                _targetBasePosition = runtimeModel.RoMap.Bases[RuntimeModel.PlayerId];
            }
                       
            if (Vector2Int.Distance(unit.Pos, _targetBasePosition) <= 2f)
            {
                _isTargetBaseEnemy = !_isTargetBaseEnemy;
            }

            if (_isFrendlyUnitsInRange && !_isPaused)
            {
                _isPaused = true;
                _timeUtil.RunDelayed(0.5f, () =>
                {
                    _isPaused = false;
                    Debug.Log($"Пауза завершена - {_isPaused}");
                });

                _activePath = new AStarPathFinding(runtimeModel, unit.Pos, _targetBasePosition);
                return _activePath.GetNextStepFrom(unit.Pos);
            }

            if(!_isPaused)
            {
                _activePath = new AStarPathFinding(runtimeModel, unit.Pos, _targetBasePosition);
                return _activePath.GetNextStepFrom(unit.Pos);
            } 
            else
            {
                return unit.Pos;                      
            }
        }

        protected override List<Vector2Int> SelectTargets()
        {
            List<Vector2Int> result = GetReachableTargets();

            while (result.Count > 1)
            {
                result.RemoveAt(result.Count - 1);
            }
            return result;
        }

        public override void Update(float deltaTime, float time)
        {
            _timeUtil.RunDelayed(3f, () =>
                {
                    _isActiveBuffUnit = true;
                }); 

            _friendUnits = GetFriendlyUnit();

            if(_friendUnits.Count != 0 && _isActiveBuffUnit)
            {
                foreach (var u in _friendUnits)
                {                   
                    if (u == this.unit || _buffService._buffs.ContainsKey(u.UnitID)) continue; // если юнит является самим собой, или юнит уже под баффом, пропускаем

                    _buffService.TempBuff(u.UnitID, _buffService.GetRandomBuff(), 3f);
                    _vfxView.PlayVFX(u.Pos, VFXView.VFXType.BuffApplied);                                     
                }
            }
        }
        
        private List<IReadOnlyUnit> GetFriendlyUnit()
        {           
            var units = new List<IReadOnlyUnit>();
            var pos = unit.Pos;

            foreach (var friendUnit in runtimeModel.RoPlayerUnits)
            {
                if(Vector2Int.Distance(this.unit.Pos, friendUnit.Pos) < 2f)
                {
                    _isFrendlyUnitsInRange = true;
                    units.Add(friendUnit);
                } else
                {
                    _isFrendlyUnitsInRange = false;
                }
            }

            return units;
        }
    }
}