using Model.Runtime.Projectiles;
using System.Collections;
using System.Collections.Generic;
using UnitBrains.Player;
using UnityEngine;

public class ThirdUnitBrain : DefaultPlayerUnitBrain
{


    public override string TargetUnitName => "Ironclad Behemoth";

    private bool isMooving = true;
    //private bool inChange = false;
    private float preparingTime = 1f;
    private float cooldownTime = 0;
    private bool isPossibleToShoot = false;


    protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
    {
        if (isPossibleToShoot )
        {
            var projectile = CreateProjectile(forTarget);
            AddProjectileToList(projectile, intoList);
        }
    }
    public override Vector2Int GetNextStep()
    {
       
        
        if (!isMooving)
        {
            return unit.Pos;
        }
        else
        {
            return base.GetNextStep();
        }

    }


    public override void Update(float deltaTime, float time)
    {

        if (SearcherTaget())
        {
            isMooving = false;
            cooldownTime += Time.deltaTime;
            float t = cooldownTime / (preparingTime / 10);
            Debug.Log(t + "III");
            if (t >= 1)
            {
               
                ShootMode();
                
            }
        }
        else if (!SearcherTaget())
        {
            isPossibleToShoot = false;
            cooldownTime += Time.deltaTime;
            float t = cooldownTime / (preparingTime / 10);
            Debug.Log(t);
            if (t >= 2)
            {
                
                Marchmode();
                
            }
        }
    }
    private bool SearcherTaget()
    {
        var targets = GetReachableTargets();
        if (targets.Count > 0)
            return true;
        else return false;
    }
    
   
    private void ShootMode()
    {
        isPossibleToShoot = true ;
        isMooving = false ;
        cooldownTime = 0;
    }
    private void Marchmode()
    {
        isPossibleToShoot = false ;
        isMooving = true;
        cooldownTime = 0;
    }
}
