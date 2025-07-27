using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Systems.BuffSystem
{
    public class BuffSystem : MonoBehaviour
    {
        private class TimedBuff
        {
            public ModifiableParams Target;
            public IBuffEffect Effect;
            public float TimeRemaining;
        }

        private readonly List<TimedBuff> _active = new();

        public void ApplyBuff(ModifiableParams target, IBuffEffect effect)
        {
            if (effect is IConditionalBuff cond && !cond.CanApplyTo(target))
            {
                return;
            }

            target.AddBuff(effect);
            Debug.Log($"Applied buff, {effect} to {target.Owner.Config.Name}");

            _active.Add(new TimedBuff
            {
                Target = target,
                Effect = effect,
                TimeRemaining = effect.Duration
            });
        }

        public void RemoveBuff(ModifiableParams target, IBuffEffect effect)
        {
            target.RemoveBuff(effect);
            _active.RemoveAll(x => x.Target == target && x.Effect == effect);
        }

        public void RemoveAllBuffs(ModifiableParams target)
        {
            target.RemoveAllBuffs();
            _active.RemoveAll(x => x.Target == target);
        }
        
        public bool HasAnyBuff(ModifiableParams target) {
            return _active.Any(x => x.Target == target);
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;

            for (int i = _active.Count - 1; i >= 0; i--)
            {
                var buff = _active[i];

                if (buff.Target.Owner.IsDead)
                {
                    buff.Target.RemoveAllBuffs();
                    _active.RemoveAll(x => x.Target == buff.Target);
                    continue;
                }

                buff.TimeRemaining -= deltaTime;
                if (buff.TimeRemaining <= 0f)
                {
                    buff.Target.RemoveBuff(buff.Effect);
                    _active.RemoveAt(i);
                    Debug.Log($"Removed buff, {buff.Effect} to {buff.Target.Owner.Config.Name}");
                }
            }
        }
    }
}