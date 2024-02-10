using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace View
{
    public class VFXView : MonoBehaviour
    {
        public enum VFXType
        {
            UnitHit,
            UnitDestroyed,
            BuffApplied,
        }
        
        [Serializable]
        public class VFXConfig
        {
            public VFXType Type;
            public GameObject Prefab;
            public float ReuseDelay = 2f;
        }

        [SerializeField] private float _vfxHeight = 1f;
        [SerializeField] private VFXConfig[] _vfxConfigs;
        
        private readonly Dictionary<VFXType, VFXConfig> _vfxConfigsByType = new();
        private readonly Dictionary<VFXType, Stack<GameObject>> _vfxPool = new();

        public void PlayVFX(Vector2Int cell, VFXType type)
        {
            if (!_vfxConfigsByType.TryGetValue(type, out var config))
                return;

            var vfx = SetupInstanceAt(type, cell);
            if (vfx != null)
                StartCoroutine(PoolAfter(config.ReuseDelay, type, vfx));
        }
        
        private void Awake()
        {
            foreach (var vfxConfig in _vfxConfigs)
            {
                _vfxConfigsByType[vfxConfig.Type] = vfxConfig;
            }
        }

        private GameObject SetupInstanceAt(VFXType type, Vector2Int cell)
        {
            GameObject instance = null;
            if (_vfxPool.TryGetValue(type, out var pool) && pool.Count > 0)
            {
                instance = pool.Pop();
            }
            else if (_vfxConfigsByType.TryGetValue(type, out var config))
            {
                instance = Instantiate(config.Prefab);
            }

            if (instance == null)
            {
                Debug.LogError($"Unsupported effect type {type}");
                return null;
            }

            var t = instance.transform;
            t.position = Gameplay3dView.ToWorldPosition(cell, _vfxHeight);
            t.rotation = Quaternion.identity;
            
            instance.SetActive(true);
            PlayParticles(instance);
            return instance;
        }

        private IEnumerator PoolAfter(float delay, VFXType type, GameObject vfx)
        {
            yield return new WaitForSeconds(delay);
            StopParticles(vfx);
            vfx.SetActive(false);

            if (!_vfxPool.TryGetValue(type, out var pool))
            {
                pool = new Stack<GameObject>();
                _vfxPool[type] = pool;
            }

            pool.Push(vfx);
        }
        
        private void PlayParticles(GameObject vfx)
        {
            foreach (var ps in vfx.GetComponentsInChildren<ParticleSystem>())
            {
                ps.Play();
            }
        }
        
        private void StopParticles(GameObject vfx)
        {
            foreach (var ps in vfx.GetComponentsInChildren<ParticleSystem>())
            {
                ps.Stop();
                ps.Clear();
            }
        }
    }
}