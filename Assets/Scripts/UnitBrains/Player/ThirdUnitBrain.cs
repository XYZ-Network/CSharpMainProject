using Model;
using Model.Runtime;
using System.Collections.Generic;
using UnitBrains.Player;
using UnityEngine;
using static UnityEngine.UI.CanvasScaler;

public class ThirdUnitBrain : DefaultPlayerUnitBrain
{
    public override string TargetUnitName => "Ironclad Behemoth";

    private enum State { Moving, Attacking, Transition }
    private State currentState = State.Moving;
    private float transitionTime;
    private const float TransitionDuration = 0.1f;

    public override void Update(float deltaTime, float time)
    {
        if (currentState == State.Transition)
        {
            transitionTime += deltaTime;
            if (transitionTime >= TransitionDuration)
            {
                currentState = HasTargets() ? State.Attacking : State.Moving;
                transitionTime = 0f;
            }
            return;
        }

        bool hasTargets = HasTargets();

        if (currentState == State.Moving && hasTargets)
        {
            currentState = State.Transition;
            transitionTime = 0f;
        }
        else if (currentState == State.Attacking && !hasTargets)
        {
            currentState = State.Transition;
            transitionTime = 0f;
        }
    }

    public override Vector2Int GetNextStep()
    {
        return currentState == State.Moving ? base.GetNextStep() : unit.Pos;
    }

    protected override List<Vector2Int> SelectTargets()
    {
        return currentState == State.Attacking ? base.SelectTargets() : new List<Vector2Int>();
    }

    private bool HasTargets()
    {
        return base.SelectTargets().Count > 0;
    }
}