using Model;
using Model.Runtime;
using Model.Runtime.ReadOnly;
using System;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

public class ActionGenerator
{
    private static ActionGenerator _actionGenerator;
    private IReadOnlyRuntimeModel _runtimeModel;
    private TimeUtil _timeUtil;
    private List<ActionRecomendation> _recomendations;
    private int _middleMap;

    private Vector2Int[] _directions = {
            Vector2Int.down,
            Vector2Int.up,
            Vector2Int.left,
            Vector2Int.right
        };

    private ActionGenerator()
    {
        _runtimeModel = ServiceLocator.Get<IReadOnlyRuntimeModel>();
        _timeUtil = ServiceLocator.Get<TimeUtil>();
        _recomendations = new List<ActionRecomendation>
        {
            new ActionRecomendation(),
            new ActionRecomendation()
        };
        _middleMap = _runtimeModel.RoMap.Width / 2;
        CalcRecomendations();
        _timeUtil.AddFixedUpdateAction((deltaTime) => CalcRecomendations());
    }

    public static ActionGenerator GetInstance()
    {
        if (_actionGenerator == null)
            _actionGenerator = new ActionGenerator();
        return _actionGenerator; 
    }

    public IReadOnlyUnit GetRecomendedTarget(int playerId)
    {
        if (_recomendations.Count <= playerId)
        {
            return null;
        }
        return _recomendations[playerId].Target;
    }

    public Vector2Int GetRecomendedStep(int playerId, Unit unit)
    {
        if (_recomendations.Count <= playerId)
        {
            return unit.Pos;
        }
        var recomendation = _recomendations[playerId];
        if (!recomendation.UseRange)
        {
            return recomendation.Step;
        }
        var step = recomendation.Step;
        var attackRange = unit.Config.AttackRange;

        int range = Mathf.FloorToInt(attackRange);

        for (int dx = -range; dx <= range; dx++)
        {
            int dyMax = range - Math.Abs(dx);
            for (int dy = -dyMax; dy <= dyMax; dy++)
            {
                if (Math.Abs(dx) + Math.Abs(dy) == range)
                {
                    Vector2Int point = new Vector2Int(step.x + dx, step.y + dy);
                    if (_runtimeModel.IsTileWalkable(point))
                    {
                        return point;
                    }
                }
            }
        }
        return unit.Pos;
    }



    private void CalcRecomendations()
    {
        var playerTarget = CalcTarget(_runtimeModel.RoMap.Bases[RuntimeModel.PlayerId], _runtimeModel.RoBotUnits);
        var playerStepPair = CalcStep(_runtimeModel.RoMap.Bases[RuntimeModel.PlayerId], _runtimeModel.RoBotUnits, _runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId]);
        _recomendations[RuntimeModel.PlayerId] = new ActionRecomendation(playerTarget, playerStepPair.Key, playerStepPair.Value);

        var enemyTarget = CalcTarget(_runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId], _runtimeModel.RoPlayerUnits);
        var enemyStepPair = CalcStep(_runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId], _runtimeModel.RoPlayerUnits, _runtimeModel.RoMap.Bases[RuntimeModel.PlayerId]);
        _recomendations[RuntimeModel.BotPlayerId] = new ActionRecomendation(enemyTarget, enemyStepPair.Key, enemyStepPair.Value);
    }

    private KeyValuePair<Vector2Int, bool> CalcStep(Vector2Int defenseBase, IEnumerable<IReadOnlyUnit> units, Vector2Int attackBase)
    {
        IReadOnlyUnit nearBaseEnemy = null;
        foreach (var unit in units)
        {
            if ((defenseBase.y > _middleMap && unit.Pos.y > _middleMap) ||
                (defenseBase.y < _middleMap && unit.Pos.y < _middleMap))
            {
                foreach (var target in _directions)
                {
                    var nextStep = target + defenseBase;
                    if (_runtimeModel.IsTileWalkable(nextStep))
                    {
                        return KeyValuePair.Create(nextStep, false);
                    }
                }
            }
            if (nearBaseEnemy == null || Vector2Int.Distance(nearBaseEnemy.Pos, defenseBase) > Vector2Int.Distance(unit.Pos, defenseBase))
            {
                nearBaseEnemy = unit;
            }
        }
        return KeyValuePair.Create(nearBaseEnemy == null ? attackBase : nearBaseEnemy.Pos, true);

    }

    private IReadOnlyUnit CalcTarget(Vector2Int defenseBase, IEnumerable<IReadOnlyUnit> units)
    {
        IReadOnlyUnit lowHealthEnemy = null;
        IReadOnlyUnit nearBaseEnemy = null;
        foreach (var unit in units)
        {
            if (lowHealthEnemy == null || lowHealthEnemy.Health > unit.Health)
            {
                lowHealthEnemy = unit;
            }
            if (defenseBase.y > _middleMap && unit.Pos.y > _middleMap || defenseBase.y < _middleMap && unit.Pos.y < _middleMap)
            {
                if (nearBaseEnemy == null)
                {
                    nearBaseEnemy = unit;
                }
                else
                {
                    nearBaseEnemy = Vector2Int.Distance(nearBaseEnemy.Pos, defenseBase) > Vector2Int.Distance(unit.Pos, defenseBase) ? unit : nearBaseEnemy;
                }
            }
        }
        return nearBaseEnemy == null ? lowHealthEnemy : nearBaseEnemy;
    }

    struct ActionRecomendation
    {
        private IReadOnlyUnit _target;
        private Vector2Int _step;
        private bool _useRange;

        public ActionRecomendation(IReadOnlyUnit target, Vector2Int step, bool useRange)
        {
            _target = target;
            _step = step;
            _useRange = useRange;
        }



        public IReadOnlyUnit Target
        {
            get
            {
                return _target;
            }
        }

        public Vector2Int Step
        {
            get
            {
                return _step;
            }
        }

        public bool UseRange
        {
            get
            {
                return _useRange;
            }
        }
    }
    
}
