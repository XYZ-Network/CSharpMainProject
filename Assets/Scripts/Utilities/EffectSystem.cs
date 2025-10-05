using Model;
using Model.Config;
using Model.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnitBrains;
using UnityEngine;
using Utilities;
using static Codice.Client.BaseCommands.Import.Commit;

namespace Assets.Scripts.Utilities
{
    public class EffectSystem : MonoBehaviour
    {
        private bool EnebledBuff = false;
        private Dictionary<Unit, List<IEffect>> unitEffects = new Dictionary<Unit, List<IEffect>>();
        private Coroutine updateCoroutine;

        private void Awake()
        {
            ServiceLocator.Register<EffectSystem>(this);
            updateCoroutine = StartCoroutine(UpdateEffectsCoroutine());
        }

        private void OnDestroy()
        {
            if (updateCoroutine != null)
                StopCoroutine(updateCoroutine);
            ServiceLocator.Unregister<EffectSystem>();
        }
        public bool TryAddEffect<T>(Effect<T> effect, T unit) where T : Unit
        {
            if (effect == null)
            {
                Debug.Log("effect пуст");
                return false;
            }
            else if (unit == null)
            {
                Debug.Log("unit пуст");
            }
            if (!effect.CanApplyTo(unit))
            {
                //Debug.LogWarning($"Эффект {effect.GetType().Name} нельзя применить к юниту {unit.Config.name}");
                return false;
            }

            AddEffect(effect, unit);
            return true;
        }
        // Добавление эффекта юниту
        public void AddEffect<T>(Effect<T> effect, T unit) where T : Unit
        {
            if (!unitEffects.ContainsKey(unit))
            {
                unitEffects[unit] = new List<IEffect>();
            }

            effect.Apply(unit);
            unitEffects[unit].Add(effect);

            Debug.Log($"Добавлен эффект {effect.GetType().Name} юниту {unit.Config.name}");
        }

        // Удаление эффекта
        public void RemoveEffect<T>(Effect<T> effect, T unit) where T : Unit
        {
            if (unitEffects.ContainsKey(unit) && unitEffects[unit].Contains(effect))
            {
                effect.Remove(unit);
                unitEffects[unit].Remove(effect);
                Debug.Log($"Удален эффект {effect.GetType().Name} у юнита {unit.Config.name}");
            }
        }

        // Корутина для обновления эффектов
        private IEnumerator UpdateEffectsCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(0.1f);
                UpdateEffects(0.1f);
            }
        }

        // Обновление длительности эффектов

        private void UpdateEffects(float deltaTime)
        {
            var unitsToRemove = new List<Unit>();

            foreach (var unitEntry in unitEffects)
            {
                var unit = unitEntry.Key;
                var effects = unitEntry.Value;

                for (int i = effects.Count - 1; i >= 0; i--)
                {
                    var effect = effects[i];
                    effect.UpdateDuration(deltaTime);

                    if (effect.IsExpired)
                    {
                        // Приводим к Effect<Unit> и вызываем Remove
                        if (effect is Effect<Unit> genericEffect)
                        {
                            genericEffect.Remove(unit);
                        }
                        effects.RemoveAt(i);
                        Debug.Log($"Эффект {effect.GetType().Name} истек у юнита {unit.Config.name}");
                    }
                }

                if (effects.Count == 0)
                {
                    unitsToRemove.Add(unit);
                }
            }

            foreach (var unit in unitsToRemove)
            {
                unitEffects.Remove(unit);
            }
        }

        // Вспомогательный метод для удаления эффекта по типу
        private void RemoveEffectByType(IEffect effect, Unit unit)
        {
            // Используем рефлексию для правильного вызова RemoveEffect
            var method = typeof(EffectSystem).GetMethod("RemoveEffect");
            var genericMethod = method.MakeGenericMethod(unit.GetType());
            genericMethod.Invoke(this, new object[] { effect, unit });
        }

        // Получение всех активных эффектов юнита
        public List<IEffect> GetUnitEffects(Unit unit)
        {
            if (unitEffects.ContainsKey(unit))
            {
                return new List<IEffect>(unitEffects[unit]);
            }
            return new List<IEffect>();
        }
        //проверка на наложенность баффа
        public bool HasEffect<T>(Unit unit) where T : IEffect
        {
            if (unitEffects.ContainsKey(unit))
            {
                return unitEffects[unit].Any(effect => effect is T);
            }
            return false;
        }
    }

    // Интерфейс для работы с эффектами без знания конкретного типа
    public interface IEffect
    {
        bool IsExpired { get; }
        float Duration { get; set; }
        void UpdateDuration(float deltaTime);
    }

    [System.Serializable]
    public abstract class Effect<T> : IEffect where T : Unit
    {
        public bool IsExpired => Duration <= 0;
        public float Duration { get; set; }

        public virtual bool CanApplyTo(Unit unit)
        {
            return unit is T;
        }
        public abstract void Apply(T unit);
        public abstract void Remove(T unit);

        public void UpdateDuration(float deltaTime)
        {
            if (Duration > 0)
                Duration -= deltaTime;
        }
    }

    // Конкретные эффекты
    public class DamageBuff : Effect<Unit>
    {
        public int DamageModifier = 2;

        public DamageBuff()
        {
            Duration = 3f;
        }
        public override bool CanApplyTo(Unit unit)
        {
            // Например, нельзя применять к уже мертвым юнитам
            return base.CanApplyTo(unit) &&
                unit.Health > 0;
        }
        public override void Apply(Unit unit)
        {
            unit.Config.Damage *= DamageModifier;
        }

        public override void Remove(Unit unit)
        {
            unit.Config.Damage /= DamageModifier;
        }
    }

    public class SpeedBuff : Effect<Unit>
    {
        public float SpeedModifier = 1.5f;

        public SpeedBuff()
        {
            Duration = 3f;
        }
        public override bool CanApplyTo(Unit unit)
        {
            // Например, нельзя применять к уже мертвым юнитам
            return base.CanApplyTo(unit) &&
                unit.Health > 0;
        }
        public override void Apply(Unit unit)
        {
            unit.Config.MoveDelay *= SpeedModifier;
        }

        public override void Remove(Unit unit)
        {
            unit.Config.MoveDelay /= SpeedModifier;
        }
    }
    public class AttackSpeedBuff : Effect<Unit>
    {
        public float attackDelayModifier = 1.5f;

        public AttackSpeedBuff()
        {
            Duration = 3f;
        }
        public override bool CanApplyTo(Unit unit)
        {
            // Например, нельзя применять к уже мертвым юнитам
            return base.CanApplyTo(unit) &&
                unit.Health > 0;
        }
        public override void Apply(Unit unit)
        {
            unit.Config.AttackDelay *= attackDelayModifier;
        }

        public override void Remove(Unit unit)
        {
            unit.Config.AttackDelay /= attackDelayModifier;
        }
    }
    public class DoubleShot : Effect<Unit>
    {
        public DoubleShot()
        {
            Duration = 10f;
        }

        public override bool CanApplyTo(Unit unit)
        {
            // Проверяем МОЖНО ли применить бафф
            return base.CanApplyTo(unit) &&
                unit.Health > 0 &&
                unit.Config.Name == "Cobra Commando"; 
        }

        public override void Apply(Unit unit)
        {
            unit.Config.EnebledBuff = true; 
        }

        public override void Remove(Unit unit)
        {
            unit.Config.EnebledBuff = false; 
        }
    }
    public class AttackRangeBuff : Effect<Unit>
    {
        public float attackRangeModifier = 1.5f;

        public AttackRangeBuff()
        {
            Duration = 3f;
        }

        public override bool CanApplyTo(Unit unit)
        {
            return base.CanApplyTo(unit) &&
                unit.Health > 0 &&
                unit.Config.Name == "Ironclad Behemoth";
        }

        public override void Apply(Unit unit)
        {
            unit.HasBuff = true;
            unit.CurrentAttackRange = unit.OriginalAttackRange * attackRangeModifier;
            Debug.Log($"Применен buff: {unit.OriginalAttackRange} -> {unit.CurrentAttackRange}");
        }

        public override void Remove(Unit unit)
        {
            unit.HasBuff = false;
            unit.CurrentAttackRange = unit.OriginalAttackRange;
            Debug.Log($"Снят buff: {unit.CurrentAttackRange}");
        }
    }
}