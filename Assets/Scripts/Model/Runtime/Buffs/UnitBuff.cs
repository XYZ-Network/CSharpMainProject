using System;
using UnityEngine;

namespace Model.Runtime.Buffs
{
    /// <summary>
    /// Временный модификатор скоростей юнита.
    /// Значения являются множителями: 1 = без изменений, 2 = в два раза быстрее,
    /// 0.5 = в два раза медленнее.
    /// </summary>
    public sealed class UnitBuff
    {
        public string Id { get; }
        public float Duration { get; private set; }
        public float MoveSpeedMultiplier { get; }
        public float AttackSpeedMultiplier { get; }

        public bool IsExpired => Duration <= 0f;

        public UnitBuff(
            float duration,
            float moveSpeedMultiplier = 1f,
            float attackSpeedMultiplier = 1f,
            string id = null)
        {
            if (duration <= 0f)
                throw new ArgumentOutOfRangeException(nameof(duration), "Duration must be greater than zero.");

            if (moveSpeedMultiplier <= 0f)
                throw new ArgumentOutOfRangeException(nameof(moveSpeedMultiplier));

            if (attackSpeedMultiplier <= 0f)
                throw new ArgumentOutOfRangeException(nameof(attackSpeedMultiplier));

            Duration = duration;
            MoveSpeedMultiplier = moveSpeedMultiplier;
            AttackSpeedMultiplier = attackSpeedMultiplier;
            Id = string.IsNullOrWhiteSpace(id) ? Guid.NewGuid().ToString() : id;
        }

        internal void Tick(float deltaTime)
        {
            Duration = Mathf.Max(0f, Duration - deltaTime);
        }

        public static UnitBuff MoveSpeedUp(float duration, float multiplier) =>
            new(duration, multiplier, 1f);

        public static UnitBuff AttackSpeedUp(float duration, float multiplier) =>
            new(duration, 1f, multiplier);

        public static UnitBuff MoveSpeedDown(float duration, float multiplier) =>
            new(duration, multiplier, 1f);

        public static UnitBuff AttackSpeedDown(float duration, float multiplier) =>
            new(duration, 1f, multiplier);
    }
}
