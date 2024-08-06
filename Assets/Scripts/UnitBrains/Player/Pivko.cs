using Assets.Scripts.Utilities.BuffManager;
using Model;
using Model.Runtime.Projectiles;
using Model.Runtime.ReadOnly;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnitBrains.Pathfinding;
using UnitBrains.Player;
using UnityEngine;
using Utilities;
using View;

public class Pivko : DefaultPlayerUnitBrain
{

    public override string TargetUnitName => "Temnoe Ne Filtrovanoe";
    private float delayBuff = 0.5f;
    private float delayMove = 0.0f;

    protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
    {
        //GDE SNARIADI?
        //Resp: За место генерации снарядов, будем арскидывать бафы в цели
        return;
       
    }
    public override Vector2Int GetNextStep()
    {

        if (delayMove > 0.5f)
            return unit.Pos;

        return base.GetNextStep();
         
    }

    public override void Update(float deltaTime, float time)
    {
 
        delayBuff -= deltaTime;
        delayMove -= deltaTime;

        if (delayBuff > 0)
        {
            return;
        }
       
        IReadOnlyUnit[] allFriendlyTargets = runtimeModel.RoUnits
                .Where(u => u.Config.IsPlayerUnit == IsPlayerUnitBrain && u.Pos != unit.Pos).ToArray();

        if (allFriendlyTargets.Length > 0)
        {
            IReadOnlyUnit unit = allFriendlyTargets[Random.Range(0, allFriendlyTargets.Length-1)];
        
            ServiceLocator.Get<EffectsManager>().addEffect((Model.Runtime.Unit)unit, new FireUpEffect((Model.Runtime.Unit)unit));
            ServiceLocator.Get<VFXView>().PlayVFX(unit.Pos, VFXView.VFXType.BuffApplied);

            delayBuff = 0.5f;
            delayMove = 0.5f;
        }


    }





}
