using Codice.Client.BaseCommands;
using Model.Runtime;
using Model.Runtime.ReadOnly;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnitBrains.Player;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Utilities;
using View;
using System.Timers;

public class FourthUnitBrain : DefaultPlayerUnitBrain
{
    public override string TargetUnitName => "Tactical Mamba";

    private bool _timerIsStarted = false;

    private EffectsForUnits _effectsForUnits = ServiceLocator.Get<EffectsForUnits>();
    private VFXView _vfxView = ServiceLocator.Get<VFXView>();
    private List<Vector2Int> WithoutTargets = new List<Vector2Int>();

    public override Vector2Int GetNextStep()
    {
        if (!_timerIsStarted)
            return base.GetNextStep();
        else
            return unit.Pos;
    }

    protected override List<Vector2Int> SelectTargets()
    {
            return WithoutTargets;
    }

    public override void Update(float deltaTime, float time)
    {
        var result = GetAllAlliedUnits();
        if (result.Any() && !_timerIsStarted)
        {
            foreach (Unit target in result)
            {
                if (IsTargetInRange(target.Pos) && !target.IsBuffed && target != unit)
                {
                    if (_effectsForUnits.TryApplyEffectToUnit(target, 10000))
                    {
                        SetTimer(500);
                        _vfxView.PlayVFX(unit.Pos, VFXView.VFXType.BuffApplied);
                        return;
                    }
                    continue;
                }
            }
        }
    }

    private void SetTimer(int durationInMs)
    {
        System.Timers.Timer timer = new System.Timers.Timer(durationInMs);
        timer.Elapsed += OnTimedEvent;
        timer.AutoReset = false;
        timer.Enabled = true;
        _timerIsStarted = true;

        void OnTimedEvent(System.Object source, ElapsedEventArgs e)
        {
            _timerIsStarted = false;
            timer.Stop();
            timer.Dispose();
        }
    }
}
