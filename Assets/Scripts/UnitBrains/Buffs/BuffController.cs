using Model.Runtime;
using Model.Runtime.ReadOnly;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitBrains;
using UnityEngine;
using Utilities;
using View;

namespace Assets.Scripts.UnitBrains.Buffs
{
    public class BuffController : MonoBehaviour
    {
        private Dictionary<IReadOnlyUnit, Dictionary<string, Buff>> _buffs = new Dictionary<IReadOnlyUnit, Dictionary<string, Buff>>();

        private Coroutine _coroutine;

        private VFXView _vfxView;
        public static BuffController Create()
        {
            var go = new GameObject("BuffController");
            DontDestroyOnLoad(go);
            return go.AddComponent<BuffController>();
        }

        private void Start()
        {
            _coroutine = StartCoroutine(UpdateBuffs());
            ServiceLocator.Register<BuffController>(this);
            _vfxView = ServiceLocator.Get<VFXView>();
            Debug.Log("[BUFFS] Registered buff controller");
        }

        public void ClearAllBuffs()
        {
            _buffs.Clear();
        }

        public void AddBuffToUnit(IReadOnlyUnit unit,Buff buff) {
            if(!_buffs.ContainsKey(unit)) {
                _buffs.Add(unit, new Dictionary<string, Buff>());
            }
            var unitBuffs = _buffs[unit];
            if (!unitBuffs.ContainsKey(buff.Id)) {
                unitBuffs.Add(buff.Id, buff);
                _vfxView.PlayVFX(unit.Pos, VFXView.VFXType.BuffApplied);
                Debug.Log($"Added buff: {buff.Id}");
            }
        }

        public float GetBuffModifierForUnit(IReadOnlyUnit unit, Buff.BuffType buffType) {
            float modifier = 1f;
            if(!_buffs.ContainsKey(unit)) { 
                return modifier;
            }
            foreach (var buff in _buffs[unit].Values)
            {
                if(buff.Type == buffType) {
                    modifier *= buff.Modifier;
                }
            }
            return modifier;
        }
        public bool IsUnitHaveBuff<T>(IReadOnlyUnit unit)
        {
            if (!_buffs.ContainsKey(unit))
            {
                return false;
            }
            foreach (var buff in _buffs[unit].Values)
            {
                if (buff.GetType().Equals(typeof(T)))
                {
                    return true;
                }
            }
            return false;
        }

        private void OnDestroy()
        {
            StopCoroutine(_coroutine);
        }

        private IEnumerator UpdateBuffs() {
            while (true)
            {
                foreach (var unit in _buffs.Keys)
                {
                    UpdateUnitBuffs(unit);
                }
                yield return new WaitForFixedUpdate();
            }
        }

        private void UpdateUnitBuffs(IReadOnlyUnit unit) {
            var unitBuffs = new Dictionary<string, Buff>(_buffs[unit]);

            foreach (var item in unitBuffs) {
            
                var expiredList = new HashSet<string>();
                foreach (var buff in unitBuffs.Values)
                {
                    buff.ReduceTimeRemains(Time.fixedDeltaTime);
                    if(buff.IsTimeExpired) {
                        expiredList.Add(buff.Id);
                        Debug.Log($"Buff: {buff.Id} removed");
                    }
                }
                foreach (var id in expiredList)
                {
                    if (unitBuffs.ContainsKey(id))
                    {
                        _buffs[unit].Remove(id);
                    }
                }
            }
        }
    }
}
