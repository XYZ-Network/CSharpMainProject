using UnityEngine;

namespace ALTUSHKA.TheAtlas.Samples
{
    // NOTE: These scripts are for demonstration purposes of "The Atlas" plugin only.
    // They do not represent best coding practices.

    public enum UnitType { Player, Enemy, NPC, Boss }

    public interface IDamageable
    {
        float Health { get; set; }
        void TakeDamage(float amount);
    }

    public struct DamageData
    {
        public float amount;
        public Vector3 hitPoint;
    }
}