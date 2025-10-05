using Assets.Scripts.Utilities;
using Codice.Client.BaseCommands;
using Model;
using Model.Runtime;
using Model.Runtime.Projectiles;
using Model.Runtime.ReadOnly;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnitBrains.Pathfinding;
using UnitBrains.Player;
using UnityEngine;
using UnityEngine.UIElements;
using Utilities;
using View;
using static UnityEngine.GraphicsBuffer;

public class FourthUnitBrain : DefaultPlayerUnitBrain
{
    public override string TargetUnitName => "Command Buffer";
    private bool Marsh = true;
    private bool GetBuff = false;
    private bool SwitchMod;
    private float SwitchModTime = 0f;
    private float _switchModTime = 1f;

    VFXView _vfxView = ServiceLocator.Get<VFXView>();
    
    

    DoubleShot _doubleShot = new DoubleShot();
    AttackRangeBuff _attackRangeBuff = new AttackRangeBuff();

    private EffectSystem _effectSystem = ServiceLocator.Get<EffectSystem>();

    //RuntimeModel _runtimeModel= ServiceLocator.Get<RuntimeModel>();

    List<Vector2Int> allPlayerUnit = new List<Vector2Int>();
    List<Vector2Int> result = new List<Vector2Int>();

    // Метод для удаления всех баффов с союзных юнитов
    private void RemoveAllBuffsFromAllies()
    {
        foreach (var target in Unit.AlLUnitPlayer)
        {
            if (target.Config.Name != "Command Buffer")
            {
                RemoveSpecificBuffsFromUnit(target);
            }
        }
    }

    // Метод для удаления конкретных баффов с юнита
    private void RemoveSpecificBuffsFromUnit(Unit target)
    {
        var effects = _effectSystem.GetUnitEffects(target);

        // Удаляем AttackRangeBuff
        var rangeBuffs = effects.Where(e => e is AttackRangeBuff).Cast<AttackRangeBuff>().ToList();
        foreach (var rangeBuff in rangeBuffs)
        {
            _effectSystem.RemoveEffect(rangeBuff, target);
        }

        // Удаляем DoubleShot
        var doubleShots = effects.Where(e => e is DoubleShot).Cast<DoubleShot>().ToList();
        foreach (var doubleShot in doubleShots)
        {
            _effectSystem.RemoveEffect(doubleShot, target);
        }

        
    }
    private void GenerateBuffsForCommand(Unit forTarget)
    {

        if (GetBuff && forTarget.Config.Name != "Command Buffer")
        {
            // Проверяем, нет ли уже такого баффа
            var effects = _effectSystem.GetUnitEffects(forTarget);
            bool hasRangeBuff = effects.Any(e => e is AttackRangeBuff);
            bool hasDoubleShot = effects.Any(e => e is DoubleShot);

            if (!hasRangeBuff)
            {
                var rangeBuff = new AttackRangeBuff();
                _effectSystem.TryAddEffect(rangeBuff, forTarget);
            }

            if (!hasDoubleShot)
            {
                var doubleShot = new DoubleShot();
                _effectSystem.TryAddEffect(doubleShot, forTarget);
            }
            

            _vfxView.PlayVFX(forTarget.Pos, VFXView.VFXType.BuffApplied);
        }
        


    }

    private IEnumerable<Vector2Int> GetCommandTargets()
    {
        return runtimeModel.RoUnits
            .Where(u => u.Config.IsPlayerUnit == IsPlayerUnitBrain && u.Config.Name!="Command Buffer")
            .Select(u => u.Pos)
            .Append(runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId]);
    }
    public override Vector2Int GetNextStep()
    {
        if (runtimeModel.Stage == RuntimeModel.GameStage.Finished)
        {
            RemoveAllBuffsFromAllies();
        }

        if (!Marsh)
        {
            
            foreach (var target in Unit.AlLUnitPlayer)
            {
               
                GenerateBuffsForCommand(target);
            }
            


            return unit.Pos;
        }
       
        else
        {
            //return base.GetNextStep();
            return unit.Pos.CalcNextStepTowards(runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId]);
        }

    }
    protected override List<Vector2Int> SelectTargets()
    {


        var currentResult = new List<Vector2Int>(); 
        allPlayerUnit = GetCommandTargets().ToList();

        foreach (var target in allPlayerUnit)
        {
            if (IsTargetInRange(target))
            {
                currentResult.Add(target);
            }
        }

        // Оставляем только первую цель
        while (currentResult.Count > 1)
        {
            currentResult.RemoveAt(currentResult.Count - 1);
        }

        return currentResult; // Возвращаем новый список
    }

    
    public override void Update(float deltaTime, float time)
    {
        result = SelectTargets(); // Пересчитываем цели

        if (result.Count > 0)
        {
            Marsh = false;
            SwitchModTime += deltaTime;
            if (SwitchModTime >= _switchModTime / 10)
            {
                SwitchModTime = 0;
                GetBuffMod();
            }
        }
        else
        {
            GetBuff = false;
            SwitchModTime += deltaTime;
            if (SwitchModTime >= _switchModTime / 10)
            {
                SwitchModTime = 0;
                MarshMod();
            }
        }
        //if (result.Count > 0)
        //{
        //    Marsh = false;
        //    SwitchModTime += Time.deltaTime;
        //    float t = SwitchModTime / (_switchModTime / 10);
        //    if (t >= 1)
        //    {
        //        SwitchModTime = 0;
        //        GetBuffMod();

        //    }
        //}
        //else
        //{
        //    GetBuff = false;
        //    SwitchModTime += Time.deltaTime;
        //    float t = SwitchModTime / (_switchModTime / 10);
        //    if (t >= 1)
        //    {
        //        SwitchModTime = 0;
        //        MarshMod();
        //    }
        //}


    }
    public void MarshMod()
    {
        Marsh = true;
        GetBuff = false;

    }
    public void GetBuffMod()
    {
        GetBuff = true;
        Marsh = false;

    }
   

}