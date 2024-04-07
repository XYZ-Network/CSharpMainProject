using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using View;

namespace Model.Config
{
    [Serializable]
    public class Settings
    {
        [SerializeField] private int _mapWidth;
        [SerializeField] private int _mapHeight;
        [SerializeField] private float _mapMinDensity;
        [SerializeField] private float _mapMaxDensity;
        [SerializeField] private int _baseLevelMoney;
        [SerializeField] private int _levelMoneyIncrement;
        [SerializeField] private int _botMoneyAdvantagePerLevel;
        [SerializeField] private int _botMoneyAdvantageLevelShift;
        [SerializeField] private int _mainBaseMaxHp;

        private readonly Dictionary<UnitConfig, UnitView> _enemyUnits = new();
        private readonly Dictionary<UnitConfig, UnitView> _playerUnits = new();
        private readonly Dictionary<string, ProjectileView> _projectiles = new();
        private readonly List<TileView> _tilePrefabs = new();

        public const int PlayersCount = 2;
        
        public int MapWidth => _mapWidth;
        public int MapHeight => _mapHeight;
        public float MapMinDensity => _mapMinDensity;
        public float MapMaxDensity => _mapMaxDensity;
        public int BaseLevelMoney => _baseLevelMoney;
        public int LevelMoneyIncrement => _levelMoneyIncrement;
        public int BotMoneyAdvantagePerLevel => _botMoneyAdvantagePerLevel;
        public int BotMoneyAdvantageLevelShift => _botMoneyAdvantageLevelShift;
        public int MainBaseMaxHp => _mainBaseMaxHp;
        
        public IReadOnlyDictionary<UnitConfig, UnitView> EnemyUnits => _enemyUnits;
        public IReadOnlyDictionary<UnitConfig, UnitView> PlayerUnits => _playerUnits;
        public IReadOnlyDictionary<string, ProjectileView> Projectiles => _projectiles;
        public IReadOnlyList<TileView> TilePrefabs => _tilePrefabs;

        public void LoadPrefabs()
        {
            LoadUnitInto(_enemyUnits, "EnemyUnits");
            LoadUnitInto(_playerUnits, "PlayerUnits");
            LoadProjectiles();
            _tilePrefabs.AddRange(Resources.LoadAll<TileView>("Tiles"));
        }

        public int GetCheapestEnemyUnitCost()
        {
            return _enemyUnits.Keys.Select(unitConfig => unitConfig.Cost).Min();
        }

        public int GetCheapestPlayerUnitCost()
        {
            return _playerUnits.Keys.Select(unitConfig => unitConfig.Cost).Min();
        }

        private void LoadUnitInto(Dictionary<UnitConfig, UnitView> target, string resourcePath)
        {
            foreach (var unitConfig in Resources.LoadAll<UnitConfig>(resourcePath))
            {
                var view = unitConfig.GetComponent<UnitView>();
                if (view == null)
                {
                    Debug.LogError($"Could not get UnitView from unit config '{unitConfig.name}'");
                    continue;
                }
                
                target.Add(unitConfig, view);
            }
        }

        private void LoadProjectiles()
        {
            foreach (var projectilePrefab in Resources.LoadAll<ProjectileView>("Projectiles"))
            {
                _projectiles.Add(projectilePrefab.ProjTypeName, projectilePrefab);
            }
        }
    }
}