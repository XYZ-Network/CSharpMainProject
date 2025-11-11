using Model.Runtime.ReadOnly;

namespace Model.Runtime
{
    public interface IBuffableUnit : IReadOnlyUnit
    {
        /// <summary>
        /// Multiplies the current move speed. Values greater than 1 speed the unit up, values between 0 and 1 slow it down.
        /// </summary>
        /// <param name="multiplier">Factor applied to move speed.</param>
        void ApplyMoveSpeedMultiplier(float multiplier);

        /// <summary>
        /// Multiplies the current attack speed. Values greater than 1 speed the unit up, values between 0 and 1 slow it down.
        /// </summary>
        /// <param name="multiplier">Factor applied to attack speed.</param>
        void ApplyAttackSpeedMultiplier(float multiplier);

        /// <summary>
        /// Adds or removes extra attack executions triggered each time the unit attacks.
        /// </summary>
        /// <param name="delta">Positive to add extra executions, negative to remove.</param>
        void ModifyExtraAttackExecutions(int delta);

        /// <summary>
        /// Multiplies current attack range.
        /// </summary>
        /// <param name="multiplier">Factor applied to attack range.</param>
        void ApplyAttackRangeMultiplier(float multiplier);

        /// <summary>
        /// The current attack range after applying all modifiers.
        /// </summary>
        float AttackRange { get; }
    }
}
