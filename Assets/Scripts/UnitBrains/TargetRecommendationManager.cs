using Codice.Client.Common.GameUI;
using Model;
using Model.Runtime.ReadOnly;
using PlasticGui.WorkspaceWindow.PendingChanges.Changelists;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities;

public class TargetRecommendationManager
{
    public static TargetRecommendationManager Instance
    {
        get
        {
            if (_instance is null)
            {
                _instance = new TargetRecommendationManager();
            }

            return _instance;
        }
    }

    private static TargetRecommendationManager _instance;

    private IReadOnlyRuntimeModel _runtimeModel;
    private TimeUtil _timeUtil;

    public IReadOnlyUnit RecommendationTarget { get; private set; }
    public Vector2Int RecommendationPosition { get; private set; }

    private TargetRecommendationManager()
    {
        _runtimeModel = ServiceLocator.Get<IReadOnlyRuntimeModel>();
        _timeUtil = ServiceLocator.Get<TimeUtil>();

        _timeUtil.AddUpdateAction(Update);
    }

    private void Update(float time)
    {
        RecommendationTarget = GetTargetRecommendationUnit();
        RecommendationPosition = GetTargetRecommendationPosition();
    }

    ~TargetRecommendationManager()
    {
        _timeUtil.RemoveUpdateAction(Update);
    }

    private IReadOnlyUnit GetTargetRecommendationUnit()
    {
        List<IReadOnlyUnit> units  = GetUnits();

        if (units.Count == 0) 
        {
            return null;
        }

        if (units.Any(unit => GetDistanceToPlayerBase(unit) <= GetDistanceToEnemyBase(unit)))
        {
            units.Sort(CompareDistanceToPlayerBase);
        }
        else
        {
            units.Sort(CompareHealthEnemy);
        }
        return units[0];
    }

    private Vector2Int GetTargetRecommendationPosition()
    {
        List<IReadOnlyUnit> units = GetUnits();

        if(units.Any(unit => GetDistanceToPlayerBase(unit) <= GetDistanceToEnemyBase(unit)))
        {
            Vector2Int dirEnemy = _runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId] - _runtimeModel.RoMap.Bases[RuntimeModel.PlayerId];

            return _runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId] + dirEnemy.SignOrZero();
        }
        else if (units.Any())
        {
            units.Sort(CompareDistanceToPlayerBase);
            return GetPositionOnPathAtRange(units[0], units[0].Config.AttackRange);
        }
        else
        {
            
            return _runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId];
        }
    }

    private Vector2Int GetPositionOnPathAtRange(IReadOnlyUnit unit, float range)
    {

        if (unit.ActivePath != null)
        {
            foreach (var pos in unit.ActivePath.GetPath())
            {
                if (Vector2Int.Distance(pos, unit.Pos) <= range)
                {
                    return pos;
                }
            }
        }
        return unit.Pos;
    }

    private List<IReadOnlyUnit> GetUnits()
    {
        return _runtimeModel.RoBotUnits.ToList();
    }

    private float GetDistanceToPlayerBase(IReadOnlyUnit Unit)
    {
        return Vector2Int.Distance(Unit.Pos, _runtimeModel.RoMap.Bases[RuntimeModel.PlayerId]);
    }

    private float GetDistanceToEnemyBase(IReadOnlyUnit Unit)
    {
        return Vector2Int.Distance(Unit.Pos, _runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId]);
    }

    private int CompareDistanceToPlayerBase(IReadOnlyUnit a, IReadOnlyUnit b)
    {
        float distanceA = GetDistanceToPlayerBase(a);
        float distanceB = GetDistanceToPlayerBase(b);

        return distanceA.CompareTo(distanceB);
    }

    private int CompareHealthEnemy(IReadOnlyUnit a, IReadOnlyUnit b)
    {
        return a.Health.CompareTo(b.Health);
    }
}
