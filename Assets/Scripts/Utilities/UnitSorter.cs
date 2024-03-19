using System;
using System.Collections.Generic;
using Model;
using Model.Runtime.ReadOnly;
using UnityEngine;

namespace Utilities
{
    public enum EBaseType
    {
        PlayerBase,
        BotBase,
        OwnBase,
    }
    
    public class UnitSorter
    {
        private IReadOnlyRuntimeModel _runtimeModel;
        private EBaseType _baseType;

        public UnitSorter()
        {
            _runtimeModel = ServiceLocator.Get<IReadOnlyRuntimeModel>();
        }
        
        
        public List<IReadOnlyUnit> SortByDistanceToBase(List<IReadOnlyUnit> units, EBaseType baseType)
        {
            _baseType = baseType;
            units.Sort(CompareByDistanceToBase);

            return units;
        }
        
        
        public List<IReadOnlyUnit> SortByHealth(List<IReadOnlyUnit> units)
        {
            units.Sort(CompareByUnitHealth);

            return units;
        }
        
        
        private float DistanceToBase(IReadOnlyUnit unit)
        {
            switch (_baseType)
            {
                case EBaseType.PlayerBase:
                {
                    return Vector2Int.Distance(unit.Pos, _runtimeModel.RoMap.Bases[RuntimeModel.PlayerId]);
                }
                case EBaseType.BotBase:
                {
                    return Vector2Int.Distance(unit.Pos, _runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId]);
                }
                case EBaseType.OwnBase:
                {
                    if (unit.Config.IsPlayerUnit)
                        return Vector2Int.Distance(unit.Pos, _runtimeModel.RoMap.Bases[RuntimeModel.PlayerId]);
                    
                    return Vector2Int.Distance(unit.Pos, _runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId]);
                }
            }
            
            throw new ArgumentException("Unknown base type: " + _baseType);
        }
        
        
        private int CompareByDistanceToBase(IReadOnlyUnit a, IReadOnlyUnit b)
        {
            var distanceA = DistanceToBase(a);
            var distanceB = DistanceToBase(b);
            return distanceA.CompareTo(distanceB);
        }
        
        
        private int CompareByUnitHealth(IReadOnlyUnit a, IReadOnlyUnit b)
        {
            var healthA = a.Health;
            var healthB = b.Health;
            return healthA.CompareTo(healthB);
        }
    }
}