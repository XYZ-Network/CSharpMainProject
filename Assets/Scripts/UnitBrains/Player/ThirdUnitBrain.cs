using System.Collections.Generic;
using System.Linq;
using UnitBrains.Player;
using UnityEngine;

public class ThirdUnitBrain : DefaultPlayerUnitBrain
{

    public override string TargetUnitName => "Ironclad Behemoth";
    private bool attack = false;
    private bool move = false;
    private bool pause = false;
    private float pauseLengthSec = 1f;
    private float pauseTimer = 0f;


    public override Vector2Int GetNextStep()
    {
        if (attack || (pause && !move))
        {
            return unit.Pos;
        }
        Vector2Int nextMove = base.GetNextStep();
        if (nextMove.Equals(unit.Pos))
        {
            if (move)
            {
                Pause();
            }
            move = false;
        } 
        else
        {
            move = true;
        }
        return nextMove;
    }

    public override void Update(float deltaTime, float time)
    {
        if (pause)
        {
            if (Time.time - pauseTimer >= pauseLengthSec)
            {
                pause = false;
            }
            else
            {
                return;
            }
        }
        base.Update(deltaTime, time);
    }

    protected override List<Vector2Int> SelectTargets()
    {
        if (move || (pause && !attack))
        {
            return new();
        }
        List<Vector2Int> nextTarget = base.SelectTargets();
        if (!nextTarget.Any())
        {
            if (attack)
            {
                Pause();
            }

            attack = false;
            return nextTarget;
        }
        attack = true;
        return nextTarget;
    }

    private void Pause()
    {
        pause = true;
        pauseTimer = Time.time;
    }
}

