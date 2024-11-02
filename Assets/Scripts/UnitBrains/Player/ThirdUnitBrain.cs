using Model;
using Model.Runtime.Projectiles;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnitBrains.Player;
using UnityEngine;

public class ThirdUnitBrain : DefaultPlayerUnitBrain
{
    public override string TargetUnitName => "Ironclad Behemoth";

    private List<Vector2Int> dangerous = new List<Vector2Int>();
    private Vector2Int selectedTarget;
    private float stunTime = 0f;
    private float idleTime= 1f;
    public int UnitID { get; private set; }
    public const int MaxTarget = 4;
    private bool tryingToMove;

   
    protected override List<Vector2Int> SelectTargets()
    {

        float minTarget = float.MaxValue;
        Vector2Int closestTarget = new Vector2Int();

        IEnumerable<Vector2Int> allTargets = GetReachableTargets();

        List<Vector2Int> result = new List<Vector2Int>(allTargets);

        if (!result.Any())
        {
            result.Clear();
            Vector2Int enemyBase = runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId];
            result.Add(enemyBase);
            closestTarget = enemyBase;
            Debug.Log(result);

        }
        else
        {
            SortByDistanceToOwnBase(result);

            List<Vector2Int> targetList = result.ToList();


            int targetIndex = targetList.Count > 1 ? UnitID % targetList.Count : 0;
            Vector2Int selectedTarget = targetList[targetIndex];


            foreach (var target in result)
            {

                float distance = DistanceToOwnBase(target);
                if (distance < minTarget)
                {
                    minTarget = distance;
                    closestTarget = target;
                    dangerous.Clear();

                }
            }
        }

        if (!IsTargetInRange(selectedTarget))
        {
            selectedTarget = closestTarget;
            return new List<Vector2Int>();

        }
        if (minTarget != float.MaxValue)
        {
            dangerous.Add(selectedTarget);
            result.Add(selectedTarget);
        }
        else
        {
            result.Clear();
        }
        return result;
    }

    

    

    public override Vector2Int GetNextStep()
    {
        if (!tryingToMove && GetReachableTargets().Count == 0)
        {
            tryingToMove = true;
            stunTime = 0f;
        }

        if (tryingToMove && stunTime >= idleTime)
        {
            return base.GetNextStep();
        }

        else if (tryingToMove)
        {
            //TODO: Получать значения гет некст степ от бемегот конфига 
            stunTime += 0.25f + Time.deltaTime;
        }
        return Vector2Int.zero;


    }


    protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
    {
        BaseProjectile projectile;

        Debug.Log(stunTime);

        if (tryingToMove && GetReachableTargets().Count != 0)
        {
            tryingToMove = false;
            stunTime = 0f;

        }
        if (!tryingToMove && GetReachableTargets().Count != 0)
        {
            stunTime += Time.deltaTime;
        }

        if (stunTime >= idleTime)
        {
            Debug.Log("ForTarget");
            projectile = CreateProjectile(forTarget);
            AddProjectileToList(projectile, intoList);
        }
    }


    void stun()
    {
        Thread.Sleep(1000);
    }

}
