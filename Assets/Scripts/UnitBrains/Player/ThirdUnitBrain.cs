using Codice.Client.Common;
using Model;
using Model.Runtime.Projectiles;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnitBrains.Player;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Utilities;

public class ThirdUnitBrain : DefaultPlayerUnitBrain
{
    public override string TargetUnitName => "Ironclad Behemoth";

    private bool _pause = false;
    private float _pauseTime = 0f;

    private int mode = 0;


    protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
    {
        var projectile = CreateProjectile(forTarget);
        AddProjectileToList(projectile, intoList);
    }
    public override Vector2Int GetNextStep()
    {
        if (mode >= 1 || _pause)
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
        if (mode == 0 || _pause)
        {
            return new List<Vector2Int>();
        }
        else
        {
            return base.SelectTargets();
        }
    }

    public override void Update(float deltaTime, float time)
    {
        int lastMode = mode;
        mode = base.SelectTargets().Count;

        if (lastMode != mode)
        {
            _pause = true;
        }

        if (_pause == true)
        {
            if (_pauseTime < 1f)
            {
                _pauseTime += deltaTime * 10;
            }
            else if (_pauseTime >= 1f)
            {
                _pause = false;
                _pauseTime = 0;
            }
        }
    }
}
