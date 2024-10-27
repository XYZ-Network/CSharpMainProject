using Model;
using Model.Runtime.ReadOnly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UIElements;
using Utilities;
using static UnityEditor.PlayerSettings;
using static UnityEngine.GraphicsBuffer;

namespace Assets.Scripts.UnitBrains
{
    public class UnitsCoordinator
    {
        IReadOnlyRuntimeModel _runtimeModel => ServiceLocator.Get<IReadOnlyRuntimeModel>();
        TimeUtil _timeUtil => ServiceLocator.Get<TimeUtil>();
        private Action<float> _updateAction;
        private static UnitsCoordinator _instance = new UnitsCoordinator();
        private List<IReadOnlyUnit> _unitsPlayer;
        private List<IReadOnlyUnit> _unitsEnemy;

        public Vector2Int PreferredPosForEnemy;
        public Vector2Int PreferredPosForPlayer;
        public Vector2Int PreferredTargetForPlayer;
        public Vector2Int PreferredTargetForEnemy;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// Я когда начинала решать подумала что надо сделать и для игрока и для бота такую механику, поэтому реализовано с учером этого ///
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public UnitsCoordinator()
        {
            _timeUtil.AddFixedUpdateAction(Update);
        }

        ~UnitsCoordinator()
        {
            _timeUtil.RemoveUpdateAction(Update);
        }
        private void Update(float deltaTime)
        {
            _unitsEnemy = _runtimeModel.RoBotUnits.ToList();
            _unitsPlayer = _runtimeModel.RoPlayerUnits.ToList();
            ChangeAllPreferrnces();
        }
        private void ChangeAllPreferrnces()
        {
            if (_unitsEnemy.Count > 0)
            {
                PreferredPosForEnemy = GetPreferredPos(false);
                PreferredTargetForEnemy = GetPreferredTarget(false);
            }
            if (_unitsPlayer.Count > 0)
            {
                PreferredPosForPlayer = GetPreferredPos(true);
                PreferredTargetForPlayer = GetPreferredTarget(true);
            }
        }

        private Vector2Int GetPreferredTarget(bool IsPlayerUnit)
        {
            Vector2Int result;
            List<IReadOnlyUnit> _units = !IsPlayerUnit ? _unitsPlayer : _unitsEnemy;

            Vector2Int _BaseCloseTarges = Vector2Int.zero;
            int _BaseCloseTargesScore = int.MaxValue;

            Vector2Int _LowHpTarges = Vector2Int.zero;
            int _LowHpTargesScore = int.MaxValue;

            foreach (var unit in _units)
            {
                if (UnitHalfWayToEnemyBase(unit.Pos, !IsPlayerUnit))
                {
                    var DistansToBase = unit.Pos - _runtimeModel.RoMap.Bases[!IsPlayerUnit ? RuntimeModel.PlayerId : RuntimeModel.BotPlayerId];
                    int DistansToBaseInt = Math.Abs(DistansToBase.x) + Math.Abs(DistansToBase.y);
                    if (_BaseCloseTargesScore > DistansToBaseInt)
                    {
                        _BaseCloseTargesScore = DistansToBaseInt;
                        _BaseCloseTarges = unit.Pos;
                    }
                }
                if (_BaseCloseTargesScore == int.MaxValue)
                {
                    if (_LowHpTargesScore > unit.Health)
                    {
                        _LowHpTargesScore = unit.Health;
                        _LowHpTarges = unit.Pos;
                    }
                }

            }
            
            if (_BaseCloseTargesScore != int.MaxValue)
                result = _BaseCloseTarges;
            else
                result = _LowHpTarges;

            return result;
        }
        private Vector2Int GetPreferredPos(bool IsPlayerUnit)
        {
            Vector2Int result = _runtimeModel.RoMap.Bases[!IsPlayerUnit ? RuntimeModel.PlayerId : RuntimeModel.BotPlayerId];
            int DistansResult = int.MaxValue;
            bool DoDefendOwnBase = false;
            List<IReadOnlyUnit> _units = !IsPlayerUnit ? _unitsPlayer : _unitsEnemy;
            foreach (var unit in _units)
            {
                if (UnitHalfWayToEnemyBase(unit.Pos, !IsPlayerUnit))
                {
                    DoDefendOwnBase = true;
                    break;
                }
                var DistansToBase = unit.Pos - _runtimeModel.RoMap.Bases[!IsPlayerUnit ? RuntimeModel.PlayerId : RuntimeModel.BotPlayerId];
                int DistansToBaseInt = Math.Abs(DistansToBase.x) + Math.Abs(DistansToBase.y);
                if (DistansResult > DistansToBaseInt)
                {
                    DistansResult = DistansToBaseInt;
                    result = unit.Pos;
                }

            }
            if (DoDefendOwnBase)
                return _runtimeModel.RoMap.Bases[IsPlayerUnit ? RuntimeModel.PlayerId : RuntimeModel.BotPlayerId];
            else return result;
        }

        private bool UnitHalfWayToEnemyBase(Vector2Int Unit, bool IsPlayerUnit)
        {
            var DistanceBetweenBase = Vector2.Distance(_runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId], _runtimeModel.RoMap.Bases[RuntimeModel.PlayerId]);
            var DistanceBetweenUnitAndEnemyBase = Vector2.Distance(Unit, _runtimeModel.RoMap.Bases[!IsPlayerUnit?RuntimeModel.PlayerId: RuntimeModel.BotPlayerId]);
            return DistanceBetweenBase / 2 >= DistanceBetweenUnitAndEnemyBase;
        }
        public bool DoPreferredStep(Model.Runtime.Unit YourUnit, Vector2Int TargetPos)
        {
            float DistanceBetweenObjectForDoStap = YourUnit.Config.AttackRange * 2;
            float distanceToTarget = Vector2Int.Distance(TargetPos, YourUnit.Pos);
            return distanceToTarget <= DistanceBetweenObjectForDoStap;
        }
    }
}
