using Model.Runtime.ReadOnly;
using UnityEngine;

namespace Model.Runtime
{
    public class MainBase : IReadOnlyBase
    {
        public int Health { get; private set; }

        public MainBase(int health)
        {
            Health = health;
        }

        public void TakeDamage(int damage)
        {
            Health -= damage;
        }
    }
}