using System.Collections;
using System.Collections.Generic;
using Model.Runtime.ReadOnly;
using UnityEngine;
using Utilities;
using View;

namespace Model.Runtime.Buffs
{
    public class BuffSystem : MonoBehaviour
    {
        private VFXView _vfxView = ServiceLocator.Get<VFXView>();
        private Dictionary<IReadOnlyUnit, List<Buff>> _buffs = new();
        
        public static BuffSystem Create()
        {
            var go = new GameObject("BuffSystem");
            DontDestroyOnLoad(go);
            return go.AddComponent<BuffSystem>();
        }

        public void AddBuffToUnit(IReadOnlyUnit unit, Buff buff)
        {
            if (!_buffs.ContainsKey(unit))
                _buffs[unit] = new List<Buff>();

            _buffs[unit].Add(buff);
            _vfxView.PlayVFX(unit.Pos, VFXView.VFXType.BuffApplied);
        }

        public List<Buff> GetActiveBuffs(IReadOnlyUnit unit)
        {
            if (_buffs.ContainsKey(unit))
                return _buffs[unit];

            return new();
        }
        
        public void Clear()
        {
            _buffs = new();
        }

        private void Start() => StartCoroutine(ProcessBuffs(0.1f));

        private IEnumerator ProcessBuffs(float time)
        {
            while (true)
            {
                foreach (var unit in _buffs.Keys)
                {
                    List<Buff> toRemoveBuffs = new();
                    foreach (var buff in _buffs[unit])
                    {
                        buff.Duration -= time;
                        if (buff.Duration <= 0)
                            toRemoveBuffs.Add(buff);
                    }

                    // Remove expired buffs
                    foreach (var toRemoveBuff in toRemoveBuffs)
                        _buffs[unit].Remove(toRemoveBuff);
                }

                yield return new WaitForSeconds(time);
            }
        }
    }
}