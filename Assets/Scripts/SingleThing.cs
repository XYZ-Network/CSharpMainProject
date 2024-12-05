using Model;
using Model.Runtime.ReadOnly;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities;
using static UnityEngine.GraphicsBuffer;

public class SingleThing
{
    private IReadOnlyRuntimeModel _model;
    private TimeUtil _timeUtil;
    private static SingleThing _instanse;
    private IReadOnlyUnit un { get; set; }
    private Vector2Int pos { get; set; }

    public  SingleThing()
    {
        _model = ServiceLocator.Get<IReadOnlyRuntimeModel>();
        _timeUtil = ServiceLocator.Get<TimeUtil>();
        _timeUtil.AddFixedUpdateAction(Subscribe);
    }

    public Vector2Int target;
    public Vector2Int position;

    public static SingleThing Instance()
    {
        if (_instanse == null)
            _instanse = new SingleThing();
        return _instanse;
    }
    private void Subscribe(float a)
    {
        un = ReccomedTarget();
        pos = ReccomendPoint();
        
    }
    
    public IReadOnlyUnit ReccomedTarget()
    {
        var enemies = _model.RoBotUnits.ToList();
        if (enemies.Count == 0)
        {
            return null;
        }
        if (IsOurPartOfMap(enemies))
        {
             enemies.Sort(CompareByDistanceToOwnBase);
        }
        else
        {
            enemies.Sort(CompareByHealth);
        }
        return enemies[0];
    }
    public Vector2Int ReccomendPoint()
    {
        var enemies = _model.RoBotUnits.ToList();
        if(enemies.Count > 0)
        {
            if (IsOurPartOfMap(enemies))
            {
                return _model.RoMap.Bases[RuntimeModel.PlayerId] + new Vector2Int(1, 0);
            }
            else
            {
                enemies.Sort(CompareByDistanceToOwnBase);
                return enemies[0].Pos;
            }
        }
        else
        {
            return _model.RoMap.Bases[RuntimeModel.BotPlayerId];
        }       
    }
    private bool IsOurPartOfMap(List<IReadOnlyUnit> enimies)
    {
        Vector2Int base_en = _model.RoMap.Bases[RuntimeModel.BotPlayerId];
        Vector2Int base_pl = _model.RoMap.Bases[RuntimeModel.PlayerId];

        var enemies = _model.RoBotUnits.ToList();
        if(enemies.Count > 0)
        {
            enemies.Sort(CompareByDistanceToOwnBase);

            var distensToOwnBase = Vector2Int.Distance(enemies[0].Pos, base_pl);
            var distensToEnBase = Vector2Int.Distance(enemies[0].Pos, base_en);

            if (distensToOwnBase < distensToEnBase)
            {
                return true;

            }
            else
            {
                return false;
            }
        }
        return false;        
    }
    float DistOwnBase(IReadOnlyUnit a)
    {
        return Vector2Int.Distance(a.Pos, _model.RoMap.Bases[RuntimeModel.BotPlayerId]);

    }
    private int CompareByDistanceToOwnBase(IReadOnlyUnit a, IReadOnlyUnit b)
    {
        var distanceA = DistOwnBase(a);
        var distanceB = DistOwnBase(b);
        return distanceA.CompareTo(distanceB);
    }

    private int CompareByHealth(IReadOnlyUnit a, IReadOnlyUnit b)
    {
        return a.Health - b.Health;
    }
}
