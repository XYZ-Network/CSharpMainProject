using Model.Runtime.ReadOnly;
using Model;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

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
        int baseId = 0;

        switch (_baseType)
        {
            case EBaseType.PlayerBase:
                baseId = RuntimeModel.PlayerId; 
                break;
            case EBaseType.BotBase:
                baseId = RuntimeModel.PlayerId;
                break;
            case EBaseType.OwnBase:
                baseId = unit.Config.IsPlayerUnit ? RuntimeModel.PlayerId : RuntimeModel.BotPlayerId;
                break;
        }

        return Vector2Int.Distance(unit.Pos, _runtimeModel.RoMap.Bases[baseId]);
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
