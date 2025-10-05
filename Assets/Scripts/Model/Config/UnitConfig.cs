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

        [SerializeField] private bool _enabledBuff = false;
       

        public bool IsPlayerUnit => _isPlayerUnit;
        public string Name => _name;
        public Sprite Icon => _icon;
        public int Cost => _cost;
        public int MaxHealth => _maxHealth;
        public float BrainUpdateInterval => _brainUpdateDelay;
        public float MoveDelay
        {
            get { return _moveDelay; }
            set { _moveDelay = value; }
        }
        public float AttackDelay
        {
            get { return _attackDelay; }
            set { _attackDelay = value; }
        }
            
        public float AttackRange
        {
            get { return _attackRange; }
            set { _attackRange = value; }
        }
            
        public ProjectileType ProjectileType => _projectileType;
        public int Damage
        {
            get { return _damage; }
            set { _damage = value; }
        }
        public bool EnebledBuff
        {
            get { return _enabledBuff; }
            set { _enabledBuff = value; }
        }
    }
}