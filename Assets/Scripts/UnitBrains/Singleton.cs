using Model;
using Model.Runtime;
using Model.Runtime.ReadOnly;
using System;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

public class PriorityActions
{
    private static PriorityActions instance;
    private IReadOnlyRuntimeModel _runtimeModel;
    private TimeUtil _timeUtil;
    private RecommendedActions _recomendations;
    private int _middleMap;
    private Vector2Int _enemyBase;
    private Vector2Int _ownBase;
    private IEnumerable<IReadOnlyUnit> _targets;

    private Vector2Int[] _directions = {
            Vector2Int.down,
            Vector2Int.up,
            Vector2Int.left,
            Vector2Int.right
        };

    public PriorityActions(bool isPlayer, IReadOnlyRuntimeModel runtimeModel, TimeUtil timeUtil)
    {
        _runtimeModel = runtimeModel;
        _timeUtil = timeUtil;
        if (isPlayer)
        {
            _ownBase = _runtimeModel.RoMap.Bases[RuntimeModel.PlayerId];
            _enemyBase = _runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId];
            _targets = _runtimeModel.RoBotUnits;
        }
        else
        {
            _ownBase = _runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId];
            _enemyBase = _runtimeModel.RoMap.Bases[RuntimeModel.PlayerId];
            _targets = _runtimeModel.RoPlayerUnits;
        }
        CalcRecomendations();
        _timeUtil.AddFixedUpdateAction((deltaTime) => CalcRecomendations());
    }

    public IReadOnlyUnit GetPriorityTarget()
    {
        return _recomendations.Target;
    }

    public Vector2Int GetPriorityStep(Unit unit)
    {
        if (!_recomendations.UseRange)
        {
            return _recomendations.Step;
        }
        var step = _recomendations.Step;
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
        var Target = CalcTarget(_ownBase, _targets);
        var Step = CalcStep(_ownBase, _targets, _enemyBase);
        _recomendations = new RecommendedActions(Target, Step.Key, Step.Value);
    }

    private KeyValuePair<Vector2Int, bool> CalcStep(Vector2Int defenseBase, IEnumerable<IReadOnlyUnit> units, Vector2Int attackBase)
    {
        IReadOnlyUnit nearBaseEnemy = null;
        foreach (var unit in units)
        {
            if ((defenseBase.y > CalcMiddleMap() && unit.Pos.y > CalcMiddleMap()) ||
                (defenseBase.y < CalcMiddleMap() && unit.Pos.y < CalcMiddleMap()))
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
            if (defenseBase.y > CalcMiddleMap() && unit.Pos.y > CalcMiddleMap() || defenseBase.y < CalcMiddleMap() && unit.Pos.y < CalcMiddleMap())
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

    private int CalcMiddleMap()
    {
        return _middleMap = _runtimeModel.RoMap.Width / 2;
    }

    struct RecommendedActions
    {
        private IReadOnlyUnit _target;
        private Vector2Int _step;
        private bool _useRange;

        public RecommendedActions(IReadOnlyUnit target, Vector2Int step, bool useRange)
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
