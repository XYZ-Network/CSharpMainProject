using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Runtime;

public class BuffService
{
    private class ActiveBuff
    {
        public BuffBase Buff;
        public Unit Unit;
        public float TimeRemaining;
    }

    private readonly List<ActiveBuff> _activeBuffs = new();

    public void Apply(BuffBase buff, Unit unit, float duration)
    {
        if (!buff.CanApply(unit))
            return;

        buff.Apply(unit);
        _activeBuffs.Add(new ActiveBuff
        {
            Buff = buff,
            Unit = unit,
            TimeRemaining = duration
        });
    }

    public void Update(float deltaTime)
    {
        for (int i = _activeBuffs.Count - 1; i >= 0; i--)
        {
            _activeBuffs[i].TimeRemaining -= deltaTime;
            if (_activeBuffs[i].TimeRemaining <= 0f)
            {
                _activeBuffs[i].Buff.Remove(_activeBuffs[i].Unit);
                _activeBuffs.RemoveAt(i);
            }
        }
    }

    public bool HasBuff(Unit unit, System.Type buffType)
    {
        foreach (var active in _activeBuffs)
        {
            if (active.Unit == unit && active.Buff.GetType() == buffType)
                return true;
        }
        return false;
    }
}