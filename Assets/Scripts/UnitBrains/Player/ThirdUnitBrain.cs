using Model;
using System.Collections.Generic;
using System.Linq;
using UnitBrains.Player;
using UnityEngine;

enum UnitMode
{
    IsShooting,
    IsRiding
}

public class ThirdUnitBrain : DefaultPlayerUnitBrain
{
    public override string TargetUnitName => "Ironclad Behemoth";
    private UnitMode _currentMode = UnitMode.IsRiding;
    private bool _changingMode;
    private float _modeChangeDelay = 0.1f;
    private float _timer;

    private List<Vector2Int> _priorityTargets = new List<Vector2Int>();

    public static int unitCounter = 0;
    private int unitNumber;
    private const int maxTargetsCount = 3;

    public ThirdUnitBrain()
    {
        unitNumber = unitCounter;
        unitCounter++;
    }

    public override void Update(float deltaTime, float time)
    {
        if (_changingMode)
        {
            _timer += Time.deltaTime;

            if (_timer >= _modeChangeDelay)
            {
                _timer = 0f;
                _changingMode = false;
            }
        }

        base.Update(deltaTime, time);
    }

    public override Vector2Int GetNextStep()
    {
        Vector2Int targetPosition = base.GetNextStep();

        if (targetPosition == unit.Pos)
        {
            if (_currentMode == UnitMode.IsRiding)
                _changingMode = true;

            _currentMode = UnitMode.IsShooting;
        }
        else
        {
            if (_currentMode == UnitMode.IsShooting)
                _changingMode = true;

            _currentMode = UnitMode.IsRiding;
        }

        return _changingMode ? unit.Pos : targetPosition;
    }

    protected override List<Vector2Int> SelectTargets()
    {
        var iD = IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.BotPlayerId;
        var baseCoords = runtimeModel.RoMap.Bases[iD];

        if (_changingMode)
            return new List<Vector2Int>();

        if (_currentMode == UnitMode.IsShooting)
        {
            _priorityTargets.Clear();
            List<Vector2Int> allTargets = GetAllTargets().ToList();
            List<Vector2Int> reachableTargets = GetReachableTargets();
            List<Vector2Int> closestTargets = new List<Vector2Int>();

            SortByDistanceToOwnBase(allTargets);

            var closestCount = maxTargetsCount > allTargets.Count ? allTargets.Count : maxTargetsCount;
            closestTargets.AddRange(allTargets.GetRange(0, closestCount));

            var targetIndex = unitNumber % maxTargetsCount;
            var indexIsExist = targetIndex < closestTargets.Count && targetIndex > 0;
            if (indexIsExist)
            {
                _priorityTargets.Add(closestTargets[targetIndex]);
            }
            else if (closestTargets.Count > 0)
            {
                _priorityTargets.Add(closestTargets[0]);
            }
            else
            {
                _priorityTargets.Add(baseCoords);
            }

            return reachableTargets.Contains(_priorityTargets.LastOrDefault()) ? _priorityTargets : reachableTargets;
        }
        return new List<Vector2Int>();
    }
}