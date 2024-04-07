using System.Collections.Generic;
using System.Linq;
using Model;
using Model.Config;
using Model.Runtime.ReadOnly;
using UnityEngine;
using Utilities;
using Random = UnityEngine.Random;

namespace View
{
    public class Gameplay3dView : MonoBehaviour
    {
        private IReadOnlyRuntimeModel _runtimeModel;
        private Settings _settings;
        private VFXView _vfxView;
        
        private readonly List<TileView> _tiles = new();
        private readonly Dictionary<IReadOnlyUnit, UnitView> _units = new();
        private readonly Dictionary<IReadOnlyProjectile, ProjectileView> _projectile = new();
        
        private readonly Dictionary<string, UnitView> _unitPrefabsPerName = new();
        private readonly HashSet<IReadOnlyUnit> _existingUnits = new();
        private readonly HashSet<IReadOnlyProjectile> _existingProjectiles = new();
        
        public static Vector3 ToWorldPosition(Vector2 cellPos, float height = 0f)
        {
            return new Vector3(2f * cellPos.x, 2f * height, 2f * cellPos.y);
        }
        
        public void Reinitialize()
        {
            _runtimeModel = ServiceLocator.Get<IReadOnlyRuntimeModel>();
            _settings = ServiceLocator.Get<Settings>();
            _vfxView = ServiceLocator.Get<VFXView>();
            LoadPrefabsIfNeeded();
            
            Clear();

            CreateTiles();
        }

        private void Update()
        {
            if (_runtimeModel == null)
                return;

            UpdateAllUnits();
            UpdateAllProjectiles();
            
            UpdateCamera();
        }

        private readonly List<IReadOnlyUnit> _unitBuffer = new();
        private void UpdateAllUnits()
        {
            _existingUnits.Clear();
            
            foreach (var unitModel in _runtimeModel.RoUnits)
            {
                _existingUnits.Add(unitModel);
                
                if (!_units.TryGetValue(unitModel, out var unitView))
                {
                    unitView = Instantiate(_unitPrefabsPerName[unitModel.Config.name], transform);
                    _units.Add(unitModel, unitView);
                }

                UpdateUnit(unitModel, unitView);
            }
            
            _unitBuffer.AddRange(_units.Keys.Where(u => !_existingUnits.Contains(u)));
            foreach (var unit in _unitBuffer)
            {
                var unitView = _units[unit];
                _units.Remove(unit);
                Destroy(unitView.gameObject);
                _vfxView.PlayVFX(unit.Pos, VFXView.VFXType.UnitDestroyed);
            }
            
            _unitBuffer.Clear();
        }

        private readonly List<IReadOnlyProjectile> _projectileBuffer = new();
        private void UpdateAllProjectiles()
        {
            _existingProjectiles.Clear();
            
            foreach (var projModel in _runtimeModel.RoProjectiles)
            {
                _existingProjectiles.Add(projModel);
                
                if (!_projectile.TryGetValue(projModel, out var projView))
                {
                    var prefab = _settings.Projectiles[projModel.GetType().Name];
                    projView = Instantiate(prefab, transform);
                    _projectile.Add(projModel, projView);
                }

                UpdateProjectile(projModel, projView);
            }

            _projectileBuffer.AddRange(_projectile.Keys.Where(u => !_existingProjectiles.Contains(u)));
            foreach (var proj in _projectileBuffer)
            {
                var projView = _projectile[proj];
                _projectile.Remove(proj);
                Destroy(projView.gameObject);
                _vfxView.PlayVFX(new Vector2Int((int)proj.Position.x, (int)proj.Position.y),
                    VFXView.VFXType.UnitHit);
            }

            _projectileBuffer.Clear();
        }

        private void UpdateUnit(IReadOnlyUnit unitModel, UnitView unitView)
        {
            var prevPosition = unitView.transform.position;
            unitView.transform.position = ToWorldPosition(unitModel.Pos);
            unitView.UpdateState(unitModel, prevPosition);
        }

        private void UpdateProjectile(IReadOnlyProjectile projModel, ProjectileView projView)
        {
            projView.transform.position = ToWorldPosition(projModel.Position, projModel.Height);
        }

        private void UpdateCamera()
        {
            if (!ServiceLocator.Contains<CameraPositioning>())
                return;
            
            var cameraPositioning = ServiceLocator.Get<CameraPositioning>();

            bool first = true;
            Bounds allCameraObjectsBounds = default;
            foreach (var obj in GetAllCameraObjects())
            {
                if (first)
                {
                    allCameraObjectsBounds = new Bounds(obj, Vector3.zero);
                    first = false;
                }
                else
                    allCameraObjectsBounds.Encapsulate(obj);
            }

            cameraPositioning.SetTarget(allCameraObjectsBounds);
        }

        private IEnumerable<Vector3> GetAllCameraObjects()
        {
            foreach (var baseTile in _runtimeModel.RoMap.Bases)
            {
                yield return ToWorldPosition(baseTile);
            }

            foreach (var unit in _units.Values)
            {
                yield return unit.transform.position;
            }
        }

        private void CreateTiles()
        {
            for (int w = 0; w < _runtimeModel.RoMap.Width; w++)
            {
                for (int h = 0; h < _runtimeModel.RoMap.Height; h++)
                {
                    var isBlocked = _runtimeModel.RoMap[w, h];
                    var isPlayerBase = _runtimeModel.RoMap.Bases[RuntimeModel.PlayerId] == new Vector2Int(w, h);
                    var isEnemyBase = _runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId] == new Vector2Int(w, h);
                    var prefabs = _settings.TilePrefabs.Where( p =>
                            p.IsBlocked == isBlocked && p.IsBaseEnemy == isEnemyBase && p.IsBasePlayer == isPlayerBase)
                        .ToList();
                    
                    if (prefabs.Count == 0)
                    {
                        Debug.LogError($"Could not find prefab for cell ({w}, {h}), isBlocked: {isBlocked}, isPlayerBase: {isPlayerBase}, isEnemyBase: {isEnemyBase}");
                        continue;
                    }
                    var tileView = Instantiate(prefabs[Random.Range(0, prefabs.Count)], transform);
                    tileView.transform.position = ToWorldPosition(new Vector2Int(w, h));
                    _tiles.Add(tileView);
                }
            }
        }
        
        private void Clear()
        {
            foreach (var unit in _units)
            {
                Destroy(unit.Value.gameObject);
            }
            
            _units.Clear();
            
            foreach (var tile in _tiles)
            {
                Destroy(tile.gameObject);
            }
            
            _tiles.Clear();
        }

        private void LoadPrefabsIfNeeded()
        {
            if (_unitPrefabsPerName.Count > 0)
                return;

            foreach (var unitPrefab in _settings.EnemyUnits.Values)
            {
                _unitPrefabsPerName.Add(unitPrefab.name, unitPrefab);
            }
            foreach (var unitPrefab in _settings.PlayerUnits.Values)
            {
                _unitPrefabsPerName.Add(unitPrefab.name, unitPrefab);
            }
        }
    }
}