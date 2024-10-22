using System.Collections.Generic;
using UnitBrains;
using UnityEngine;
using Utilities;

public class BuffService: IBuffable
{
    public CharacterStats _baseStats { get; private set; }

    private TimeUtil _timeUtil;
    public Dictionary<string, CharacterStats> _buffs {  get; private set; }
    public BuffService(CharacterStats stats, TimeUtil timeUtil)
    {
        _buffs = new Dictionary<string, CharacterStats>();
        _baseStats = stats;
        _timeUtil = timeUtil;
        Debug.Log("Create Service Buffs");
    }

    public void AddStats(string Id, CharacterStats stats)
    {
        if (!_buffs.ContainsKey(Id)) 
            _buffs[Id] = stats;      
    }

    public void ResetStats(string Id)
    {
        if(_buffs.ContainsKey(Id))
        {
            //_buffs[Id] = _baseStats;
            _buffs.Remove(Id);
        }
    }

    public void TempBuff(string Id, CharacterStats stats, float delay)
    {
        AddStats(Id, stats);
        
        _timeUtil.RunDelayed(delay, () =>
        {
            ResetStats(Id);
            Debug.Log("Reset Buffs");
        });
        
    }

    // Метод для получения баффа по ID юнита
    public CharacterStats? GetBuffByUnitID(string Id)
    {
        if (_buffs.TryGetValue(Id, out CharacterStats stats))
        {
            return stats; 
        }
        return null; 
    }

    public CharacterStats GetRandomBuff()
    {
        List<CharacterStats> buffsList = new List<CharacterStats>()
            {
                new MoveFasterBuff().ApplyBuff(_baseStats),
                new MoveSlowlyBuff().ApplyBuff(_baseStats),
                new ShootFasterBuff().ApplyBuff(_baseStats),
                new ShootSlowlyBuff().ApplyBuff(_baseStats),
            };

        CharacterStats buff = buffsList[Random.Range(0, buffsList.Count)];
        return buff;
    }
}
