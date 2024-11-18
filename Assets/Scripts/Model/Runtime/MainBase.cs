using Model.Runtime.ReadOnly;
using UnityEngine;

namespace Model.Runtime
{
    public class MainBase : IReadOnlyBase
    {
        public int Health { get; private set; }
        public Vector2Int Pos { get; private set; }

        // Существующий конструктор
        public MainBase(int health)
        {
            Health = health;
            Pos = new Vector2Int(0, 0); 
        }

        // Новый конструктор
        public MainBase(int health, Vector2Int position)
        {
            Health = health;
            Pos = position; // Устанавливаем позицию
        }

        public void TakeDamage(int damage)
        {
            Health -= damage;
        }
    }
}