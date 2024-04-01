using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnitBrains.Player;
using UnityEngine;

public class ThirdUnitBrain : DefaultPlayerUnitBrain
   
{
    public override string TargetUnitName => "Ironclad Behemoth";

    private const float TransitionDuration = 1f;
    private float transitionStartTime = 0f;
    private bool ShootingMod=false;
    private bool transittionMod=false;
    private bool hasTarget = false;

    public override Vector2Int GetNextStep()
    {
        if (ShootingMod || transittionMod)
            return unit.Pos;
        return base.GetNextStep();
    }
    protected  List<Vector2Int> SelectTarget()
    {
        var result = base.SelectTargets();
        hasTarget = result.Count > 0;
        if (!ShootingMod || transittionMod)
            result.Clear();

        return result;
    }
    public override void Update(float deltaTime, float time)
    {
        base.Update( deltaTime, time);
        if (ShootingMod && !hasTarget && !transittionMod||
            !ShootingMod && hasTarget && !transittionMod)
        {
            transittionMod = true;
            transitionStartTime =time;
        }
        if (transittionMod && time - transitionStartTime>=TransitionDuration)
        {
            ShootingMod = !ShootingMod;
            transittionMod = false;
        }
    }


}


   

  

