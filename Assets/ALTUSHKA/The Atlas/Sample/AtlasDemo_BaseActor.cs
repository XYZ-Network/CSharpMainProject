using UnityEngine;

namespace ALTUSHKA.TheAtlas.Samples
{
    // NOTE: These scripts are for demonstration purposes of "The Atlas" plugin only.
    // They do not represent best coding practices.

    public abstract class AtlasDemo_BaseActor : MonoBehaviour, IDamageable
    {
        [SerializeField] protected UnitType unitCategory;

        public AtlasDemo_Weapon AtlasDemo_Weapon;
        public AtlasDemo_Manager AtlasDemo_Manager;

        // Property with Get/Set tracking
        public float Health { get; set; }
        public float Mana;

        public virtual void TakeDamage(float amount)
        {
            Health -= amount;
            // Accessing static member of another class
            if (AtlasDemo_Manager.IsGamePaused) return;
        }

        public abstract void Move();
    }
}