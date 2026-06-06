using System.Collections.Generic;
using UnityEngine;

namespace ALTUSHKA.TheAtlas.Samples
{
    // NOTE: These scripts are for demonstration purposes of "The Atlas" plugin only.
    // They do not represent best coding practices.

    [RequireComponent(typeof(CharacterController))]
    public class AtlasDemo_Player : AtlasDemo_BaseActor
    {
        public List<AtlasDemo_Weapon> inventory;

        private void OnEnable()
        {
            if (AtlasDemo_Manager.Instance != null)
            {
                AtlasDemo_Manager.Instance.OnLevelStart += OnLevelStarted;
            }
        }

        private void OnDisable()
        {
            AtlasDemo_Manager.Instance.OnLevelStart -= OnLevelStarted;
        }

        public override void Move()
        {
            if (inventory.Count > 0)
            {
                inventory[0].Fire();
            }
        }

        private void OnLevelStarted()
        {
            unitCategory = UnitType.Player;
            Debug.Log("Player is ready!");
        }
    }
}