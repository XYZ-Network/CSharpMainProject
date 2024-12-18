using Model.Config;
using Model.Runtime;
using Model.Runtime.Projectiles;
using Model.Runtime.ReadOnly;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnitBrains.Player;
using UnityEngine;

public class FourthUnitBrain : DefaultPlayerUnitBrain
{
    public override string TargetUnitName => "Healig Behemoth";

    private const float _cooldownTime = 0.5f;
    private float _reloadTime;
    private float _reloadTimebuff;

    private bool _isCooldown = false;
    private bool _isReloadBuuf = false;

    private ActionUnit _currentAction;

    private enum ActionUnit
    {
        Move,
        buff
    }

    public override Vector2Int GetNextStep()
    {
        if (!_isCooldown && _currentAction == ActionUnit.Move)
        {
            return base.GetNextStep();
        }
        else
        {
            return unit.Pos;
        }
    }

    public override List<BaseProjectile> GetProjectiles()
    {
        return new List<BaseProjectile>();
    }

    protected override List<Vector2Int> SelectTargets()
    {
        return base.SelectTargets();
    }

    protected bool SelectUnitTargetsBuff(out Unit targetUnit)
    {
        targetUnit = null;

        var result = new List<Unit>(GetReachableTargetsUnit());
        if (result.Count > 0)
        {   targetUnit = result[0];
            return true;
        }
        return false;
    }

    public override void Update(float deltaTime, float time)
    {
        if (_isCooldown)
        {
            _reloadTime += Time.deltaTime + 0.25f;
            if (_reloadTime >= _cooldownTime)
            { 
                _reloadTime = 0f;
                _isCooldown = false;
            }
        }
        else
        {
            if (_isReloadBuuf)
            {
                _reloadTimebuff -= Time.deltaTime + 0.25f;
                if (_reloadTimebuff <= 0)
                {
                    _reloadTime = 0;
                    _isReloadBuuf = false;
                }
            }

            if (_isReloadBuuf && _currentAction == ActionUnit.buff)
            {
                _currentAction = ActionUnit.Move;
            }
            else if(!_isReloadBuuf && _currentAction == ActionUnit.Move)
            {
                _isCooldown = true;
                _currentAction = ActionUnit.buff;
            }

            if (!_isCooldown && HasUnitsInRange() && _currentAction == ActionUnit.buff 
                && !_isReloadBuuf && SelectUnitTargetsBuff(out Unit targetUnit))
            {
                _isReloadBuuf = true;
                _isCooldown = true;

                NameEffects current = NameEffects.BuffAttackSpeed;

                SystemEffectsController.StartEffectToUnit(targetUnit, current, out _reloadTimebuff);
                
                _currentAction = ActionUnit.Move;
            }
        }
    }
}
