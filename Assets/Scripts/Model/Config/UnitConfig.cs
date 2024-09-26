using Model.Runtime.Projectiles;
using UnityEngine;

namespace Model.Config
{
    public class UnitConfig : MonoBehaviour
    {
        [SerializeField] private bool _isPlayerUnit;
        [SerializeField] private string _name;
        [SerializeField] private Sprite _icon;
        [SerializeField] private int _cost;
        [SerializeField] private int _maxHealth;
        [SerializeField] private float _brainUpdateDelay = 0.25f;
        [SerializeField] private float _moveDelay = 0.25f;
        [SerializeField] private float _attackDelay = 0.75f;
        [SerializeField] private float _attackRange = 3.5f;
        [SerializeField] private ProjectileType _projectileType = ProjectileType.ArchToTile;
        [SerializeField] private int _damage = 15;
        [SerializeField] private int _unitType = 0;
        [SerializeField] public float _attackRangeBuff = 1f;
        [SerializeField] public bool _doubleShot = false;

        public bool IsPlayerUnit => _isPlayerUnit;
        public string Name => _name;
        public Sprite Icon => _icon;
        public int Cost => _cost;
        public int MaxHealth => _maxHealth;
        public float BrainUpdateInterval => _brainUpdateDelay;
        public float MoveDelay => _moveDelay;
        public float AttackDelay => _attackDelay;
        public float AttackRange => _attackRange;
        public ProjectileType ProjectileType => _projectileType;
        public int Damage => _damage;
        public int UnitType => _unitType;
        public float AttackRangeBuff => _attackRangeBuff;
        public bool DoubleShot => _doubleShot;
    }
}