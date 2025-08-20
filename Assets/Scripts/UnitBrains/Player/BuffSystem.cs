using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using Model.Runtime;
using UnityEngine;
using Model.Runtime.ReadOnly;

namespace Assets.Scripts.UnitBrains.Player
{
    public class BuffSystem : MonoBehaviour
    {
        private readonly Dictionary<Unit, List<Buff>> _unitBuffs = new();

        public void AddBuff(Unit unit, Buff buff)
        {
            if(!_unitBuffs.ContainsKey(unit))
                _unitBuffs[unit] = new List<Buff>();

            _unitBuffs[unit].Add(buff);
        }

        public float GetMoveSpeedModifier(Unit unit)
        {
            if (!_unitBuffs.ContainsKey(unit)) return 1f;
            float mod = 1f;
            foreach (var buff in _unitBuffs[unit])
                mod *= buff.MoveSpeedModifier;
            return mod;
        }

        public float GetAttackSpeedModifier(Unit unit)
        {
            if (!_unitBuffs.ContainsKey(unit)) return 1f;
            float modifier = 1.0f;
            foreach (var buff in _unitBuffs[unit])
            {
                modifier *= buff.AttackSpeedModifier;
            }
            return modifier;
        }

        private void Update()
        {
            var toRemove = new List<(Unit, Buff)> ();

            foreach (var pair in _unitBuffs)
            {
                var unit = pair.Key;
                var buffs = pair.Value;

                foreach (var buff in buffs)
                {
                    buff.Duration -= Time.deltaTime;
                    if(buff.Duration <= 0)
                        toRemove.Add((unit, buff));
                }
            }

            foreach(var (unit, buffs) in toRemove)
                _unitBuffs[unit].Remove(buffs);

        }

        public bool HasBuff(IReadOnlyUnit unit)
        {
            if (unit is Unit realUnit)
            {
                return _unitBuffs.ContainsKey(realUnit) && _unitBuffs[realUnit].Count > 0;
            }
            return false;
        }
    }
}
