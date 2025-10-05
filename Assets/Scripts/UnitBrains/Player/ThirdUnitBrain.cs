using Codice.Client.BaseCommands;
using Model;
using Model.Runtime.Projectiles;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnitBrains.Pathfinding;
using UnitBrains.Player;
using UnityEngine;
using Utilities;

public class ThirdUnitBrain : DefaultPlayerUnitBrain
{
    public override string TargetUnitName => "Ironclad Behemoth";
    private bool Marsh = true;
    private bool Shoot = false;
    private bool SwitchMod;
    private float SwitchModTime = 0f;
    private float _switchModTime = 1f;
    List<Vector2Int> result= new List<Vector2Int>();
    protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
    {

        if (Shoot)
        {
            var projectile = CreateProjectile(forTarget);
            AddProjectileToList(projectile, intoList);
        }
  
    }
    public override Vector2Int GetNextStep()
    {
        if (!Marsh) 
        {
            return unit.Pos;
        }
        else
        {
            return base.GetNextStep();
        }
      
    }
    protected override List<Vector2Int> SelectTargets()
    {
        int enemyID = IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId;
        Vector2Int enemyBase = runtimeModel.RoMap.Bases[enemyID];

        result = GetReachableTargets();

        while (result.Count > 1)
        {
            result.RemoveAt(result.Count - 1);
        }
        return result;
       
    }
    public override void Update(float deltaTime, float time)
    {
        if (result.Count > 0)
        {
            Marsh = false;
            SwitchModTime += Time.deltaTime;
            float t = SwitchModTime / (_switchModTime / 10);
            if (t >= 1)
            {
                SwitchModTime = 0;
                ShootMod();
            }
        }
        else
        {
            Shoot = false;
            SwitchModTime += Time.deltaTime;
            float t = SwitchModTime / (_switchModTime / 10);
            if (t >= 1)
            {
                SwitchModTime = 0;
                MarshMod();
            }
        }


    }
    public  void MarshMod()
    {
        Marsh = true;
        Shoot = false;
         
    }
    public  void ShootMod()
    {
        Shoot = true;
        Marsh=false;
        
    }
        //if (SwitchMod)
        //{
        //    Debug.Log($"{SwitchMod} в режиме");
        //    SwitchModTime += Time.deltaTime;

        //    float t = SwitchModTime / (_switchModTime / 10);
        //    if (t >= 1)
        //    {
        //        SwitchModTime = 0;
        //        SwitchMod = false;
        //    }
        //    Debug.Log($"{SwitchMod} в режиме");
        //}

}

