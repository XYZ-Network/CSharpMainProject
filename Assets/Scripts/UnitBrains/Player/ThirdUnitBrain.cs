using Codice.Client.BaseCommands;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnitBrains.Player;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class ThirdUnitBrain : DefaultPlayerUnitBrain
{
    public override string TargetUnitName => "Ironclad Behemoth";
    private bool IsMoving = false;
    private bool IsShooting = false;
    private int MovingCooldownCounter = 0;
    private int ShootingCooldownCounter = 0;

    private List<Vector2Int> Targets = new List<Vector2Int>();
    private List<Vector2Int> WithoutTargets = new List<Vector2Int>();


    public override Vector2Int GetNextStep()
    {
        if (!Targets.Any() && MovingCooldownCounter >= 4)
        {
            IsMoving = true;
            //
            //bool HasTargetsInDoubleRange()
            //{
            //    var attackRangeSqr = (unit.Config.AttackRange * unit.Config.AttackRange) * 2;
            //    foreach (var possibleTarget in GetAllTargets())
            //    {
            //        var diff = possibleTarget - unit.Pos;
            //        if (diff.sqrMagnitude < attackRangeSqr)
            //            return true;
            //    }

            //    return false;
            //}

            //if (HasTargetsInDoubleRange())
            //{
            //    return UnitCoordinator.GetInstance().GetTarget();
            //}

            //return UnitCoordinator.GetInstance().GetPoint();
            //
            return base.GetNextStep();
        }
        else
        {
            IsMoving = false;
            return unit.Pos;
        }
    }



    protected override List<Vector2Int> SelectTargets()
    {
        Targets.Clear();
        Targets = GetReachableTargets();

        if (Targets.Any() && ShootingCooldownCounter >= 2)
        {
            IsShooting = true;
            while (Targets.Count > 1)
            {
                Targets.RemoveAt(Targets.Count - 1);
            }
            return Targets;
        }
        else
        {
            IsShooting = false;
            return WithoutTargets;
        }

            
        
        
    }



    public override void Update(float deltaTime, float time)
    {
        if (!IsShooting)
            MovingCooldownCounter++;
        else
            MovingCooldownCounter = 0;

        if (!IsMoving)
            ShootingCooldownCounter++;
        else
            ShootingCooldownCounter = 0;
    }
}
