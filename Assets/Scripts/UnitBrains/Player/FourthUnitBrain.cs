using System;
using Model.Runtime.ReadOnly;
using Model.Runtime.Projectiles;
using System.Collections.Generic;
using UnitBrains.Player;
using Utilities;
using View;

public class FourthUnitBrain : DefaultPlayerUnitBrain
{
    public override string TargetUnitName => "Magic Damn";
    private const float BuffDuration = 5f;
    private const int ExtraShots = 1;
    private const float RangeBuffMultiplier = 1.5f;
    private static readonly System.Random Random = new();

    private float _nextBuffTime;
    private float _bufCooldown = 0.5f;

    public override List<BaseProjectile> GetProjectiles()
    {
        List<BaseProjectile> result = new();
        return result;
    }

    public override void Update(float deltaTime, float time)
    {
        if (time < _nextBuffTime)
        {
            return;
        }

        _nextBuffTime = time + _bufCooldown;

        var friends = GetUnitsInRadius(unit.AttackRange, true);
        var buffSystem = ServiceLocator.Get<BuffDebuffSystem>();
        var vfxView = ServiceLocator.Get<VFXView>();

        foreach (var friend in friends)
        {
            var buff = CreateBuffFor(friend);
            if (buff == null)
            {
                continue;
            }

            if (buffSystem.HasBuffs(friend))
            {
                continue;
            }

            buffSystem.ApplyBuff(friend, buff);
            vfxView.PlayVFX(friend.Pos, VFXView.VFXType.BuffApplied);
            break;
        }
    }

    private BuffDebuff CreateBuffFor(IReadOnlyUnit friend)
    {
        return friend.Config.Name switch
        {
            "Cobra Commando" => new DoubleShotBuff(BuffDuration, ExtraShots),
            "Ironclad Behemoth" => new AttackRangeBuff(BuffDuration, RangeBuffMultiplier),
            _ => CreateRandomGenericBuff()
        };
    }

    private BuffDebuff CreateRandomGenericBuff()
    {
        return Random.Next(0, 2) == 0
            ? new AttackBuffDebuff(BuffDuration, 1.5f)
            : new MoveBuffDebuff(BuffDuration, 1.5f);
    }
}
