using Codice.Client.Common.GameUI;
using Model;
using Model.Config;
using Model.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using UnityEngine;
using Utilities;
using static Codice.Client.BaseCommands.Import.Commit;

public class EffectsForUnits
{
    //private RuntimeModel _runtimeModel;

    private Dictionary<string, List<UnitEffect>> _allowedBuffsForUnits = new Dictionary<string, List<UnitEffect>>
    {
        {string.Empty, new List<UnitEffect> { new UnitEffect(EffectType.AttackDelay, 0.2f), new UnitEffect(EffectType.MoveDelay, 0.2f)} },
        {"Cobra Commando", new List<UnitEffect> { new UnitEffect(EffectType.DoubleAttack) } },
        {"Ironclad Behemoth", new List<UnitEffect> { new UnitEffect(EffectType.AttackRange, 5f)} },
        {"Tactical Mamba", new List<UnitEffect> () }
    };

    public EffectsForUnits()   //RuntimeModel runtimeModel
    {
        //_runtimeModel = runtimeModel;
    }

    public bool TryApplyEffectToUnit(Unit unit, int durationInMs)
    {
        List<UnitEffect> nullref = new List<UnitEffect>();
        if (_allowedBuffsForUnits.TryGetValue(unit.GiveUnitName(), out List<UnitEffect> effectTypes))
        {
            if (effectTypes.Any())
            {
                UnitEffect effectType = effectTypes[UnityEngine.Random.Range(0, effectTypes.Count)];
                SetTimerForEffect(unit, durationInMs, effectType);
                return true;
            }
            return false;
        }
        return false;
    }

    private void SetTimerForEffect(Unit unit, int durationInMs, UnitEffect effect)
    {
        System.Timers.Timer timer = new System.Timers.Timer(durationInMs);
        timer.Elapsed += OnTimedEvent;
        timer.AutoReset = false;
        timer.Enabled = true;

        unit.ChangeIsBuffedStatus(true);
        ApplyEffectImmediately(unit, effect);

        void OnTimedEvent(System.Object source, ElapsedEventArgs e)
        {
            RevertEffect(unit, effect);
            unit.ChangeIsBuffedStatus(false);

            timer.Stop();
            timer.Dispose();
        }
    }

    private void ApplyEffectImmediately(Unit unit, UnitEffect effect)
    {
        switch (effect.Type)
        {
            case EffectType.MoveDelay:
                unit.ChangeMoveDelay(unit.Config.MoveDelay * effect.Modifier);
                break;
            case EffectType.AttackDelay:
                unit.ChangeAttackDelay(unit.Config.AttackDelay * effect.Modifier);
                break;
            case EffectType.AttackRange:
                unit.ChangeAttackRange(unit.Config.AttackRange * effect.Modifier);
                break;
            case EffectType.DoubleAttack:
                unit.ChangeDoubleAttackStatus(true);
                break;
        }
    }

    private void RevertEffect(Unit unit, UnitEffect effect)
    {
        switch (effect.Type)
        {
            case EffectType.MoveDelay:
                unit.ChangeMoveDelay(unit.Config.MoveDelay);
                break;
            case EffectType.AttackDelay:
                unit.ChangeAttackDelay(unit.Config.AttackDelay);
                break;
            case EffectType.AttackRange:
                unit.ChangeAttackRange(unit.Config.AttackRange);
                break;
            case EffectType.DoubleAttack:
                unit.ChangeDoubleAttackStatus(false);
                break;
        }
    }


    private enum EffectType
    {
        MoveDelay,
        AttackDelay,
        AttackRange,
        DoubleAttack
    }
    private struct UnitEffect
    {
        public EffectType Type { get; }
        public float Modifier { get; }

        public UnitEffect(EffectType type, float modifier = 1f)
        {
            Type = type;
            Modifier = modifier;
        }
    }
}





