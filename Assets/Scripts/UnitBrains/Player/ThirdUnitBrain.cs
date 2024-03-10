using Model;
using Model.Runtime.Projectiles;
using System.Collections.Generic;
using UnitBrains.Pathfinding;
using UnitBrains.Player;
using UnityEditor;
using UnityEngine;
using static System.TimeZoneInfo;

public class ThirdUnitBrain : DefaultPlayerUnitBrain
{
    public override string TargetUnitName => "Ironclad Behemoth";
    bool isMoving = true;
    bool isShooting = false;

    private float stopTimer = 0f; 
    private float stopDuration = 1f; 

    public override Vector2Int GetNextStep()
    {
        if(isMoving)
        {
            return base.GetNextStep();

        } else
        {
            return unit.Pos;
        }
    }

    protected override List<Vector2Int> SelectTargets()
    {
        return base.SelectTargets();
    }

    protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
    {
        if(!isMoving)
        {
            AddProjectileToList(CreateProjectile(forTarget), intoList);
        } 
    }

    private void ChangeState()
    {

        if (!HasTargetsInRange())
        {

            stopTimer += Time.deltaTime;
            if (stopTimer > stopDuration)
            {
                isMoving = true;
                stopTimer = 0f;
            }
        }

        else
        {
            stopTimer += Time.deltaTime;
            if (stopTimer > stopDuration) // 
            {
                isMoving = false;
                stopTimer = 0f;
            }
        }

    }

    public override void Update(float deltaTime, float time)
    {
        base.Update(deltaTime, time);

        ChangeState();

        //Debug.Log(stopTimer);

        //Debug.Log($"isMoving: { isMoving}");
        //Debug.Log($"isShooting: {isShooting}");
        Debug.Log($"in range: {HasTargetsInRange()}");
    }
}



    










