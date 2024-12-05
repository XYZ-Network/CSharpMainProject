using Model;
using Model.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnitBrains;
using UnitBrains.Player;
using UnityEngine;
using Utilities;
using View;

public class SkyBuffer : DefaultPlayerUnitBrain
{
    public override string TargetUnitName => "Sky Buffer";
    private float reload = 2f;
    private float reload_move = 0.5f;
    private float _cooldownTime = 0f;
    private bool canBuff = true;
    private bool canMove = true;
    private VFXView vfxView;
   
    public override Vector2Int GetNextStep()
    {
        if(canMove)
        {
            return base.GetNextStep();
        }
        else
        {
            return unit.Pos;
        }
        
    }

    public override void Update(float deltaTime, float time)
    {
        
        if (HasFriendUnitsInRange() && canBuff)
        {
            AddBaff();
            canBuff = false;
            canMove = false;
        }
        else
        {
            _cooldownTime += Time.deltaTime;
            float t = _cooldownTime / (reload / 10);
            if (t >= 1)
            {
                _cooldownTime = 0;
                canBuff = true;
            }
            _cooldownTime += Time.deltaTime;
            t = _cooldownTime / (reload_move / 10);
            if (t >= 1)
            {
                _cooldownTime = 0;
                canMove = true;
            }
        }
    }
    protected override List<Vector2Int> SelectTargets()
    {
        return new List<Vector2Int>();
    }
    public void AddBaff()
    {
        vfxView = ServiceLocator.Get<VFXView>();
        SystemForBuffs _buffs = ServiceLocator.Get<SystemForBuffs>();
        List<Model.Runtime.ReadOnly.IReadOnlyUnit> listfr = new List<Model.Runtime.ReadOnly.IReadOnlyUnit>();
        listfr = GetFriendsInRange();
        while (listfr.Count > 1)
        {
            listfr.RemoveAt(listfr.Count - 1);
        }
        float bufmaybe = _buffs.Push((Unit)listfr[0], new BufSpeed<Unit>(0.5f, 0.2f));
        
        vfxView.PlayVFX(listfr[0].Pos, VFXView.VFXType.BuffApplied);
     
        


    }
    public bool HasFriendUnitsInRange()
    {
        var attackRangeSqr = unit.Config.AttackRange * unit.Config.AttackRange;

        foreach (var funit in runtimeModel.RoPlayerUnits)
        {
            var diff = funit.Pos - unit.Pos;
            return diff.sqrMagnitude <= attackRangeSqr;
        }
        return false;
    }
    private List<Model.Runtime.ReadOnly.IReadOnlyUnit> GetFriendsInRange()
    {
        List<Model.Runtime.ReadOnly.IReadOnlyUnit> listfr = new List<Model.Runtime.ReadOnly.IReadOnlyUnit> ();
        var attackRangeSqr = unit.Config.AttackRange * unit.Config.AttackRange;

        foreach (var funit in runtimeModel.RoPlayerUnits)
        {
            var diff = funit.Pos - unit.Pos;
            if(diff.sqrMagnitude <= attackRangeSqr && diff.sqrMagnitude != 0)
            {
                listfr.Add(funit);
            }
        }
        return listfr;
    }
}
