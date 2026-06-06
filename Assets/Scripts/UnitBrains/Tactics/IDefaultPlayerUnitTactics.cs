using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDefaultPlayerUnitTactics
{
    public PositionWithDanger GetPriorityTarget(bool isPlayerUnitBrain);
}
