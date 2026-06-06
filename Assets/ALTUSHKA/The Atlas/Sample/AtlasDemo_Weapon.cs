using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace ALTUSHKA.TheAtlas.Samples
{
    // NOTE: These scripts are for demonstration purposes of "The Atlas" plugin only.
    // They do not represent best coding practices.

    public class AtlasDemo_Weapon : MonoBehaviour
    {
        // Using custom struct
        public DamageData lastHit;

        public AtlasDemo_Player target;

        public void Fire()
        {
            if (target != null)
            {
                // Property Set tracking
                target.Health -= 10f;
                target.Mana = 0;
                lastHit = new DamageData { amount = 10f, hitPoint = target.transform.position };
            }
        }
    }
}