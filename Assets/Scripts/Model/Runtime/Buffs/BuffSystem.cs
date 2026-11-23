using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;
using View;

namespace Model.Runtime.Buffs
{
    public class BuffSystem : MonoBehaviour
    {
        private VFXView _vfxView = ServiceLocator.Get<VFXView>();
        private List<IBuff> _buffs = new();
        
        public static BuffSystem Create()
        {
            var go = new GameObject("BuffSystem");
            DontDestroyOnLoad(go);
            return go.AddComponent<BuffSystem>();
        }

        public void AddBuffToUnit(IBuff buff)
        {
            buff.Apply();
            _buffs.Add(buff);
            _vfxView.PlayVFX(buff.Unit.Pos, VFXView.VFXType.BuffApplied);
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
                List<IBuff> toRemoveBuffs = new();
                foreach (var buff in _buffs)
                {
                    buff.ReduceRemainingTime(time);
                    if (buff.IsExpired())
                        toRemoveBuffs.Add(buff);
                }
                
                // Remove expired buffs
                foreach (var toRemoveBuff in toRemoveBuffs)
                {
                    _buffs.Remove(toRemoveBuff);
                    toRemoveBuff.Remove();
                }

                yield return new WaitForSeconds(time);
            }
        }
    }
}