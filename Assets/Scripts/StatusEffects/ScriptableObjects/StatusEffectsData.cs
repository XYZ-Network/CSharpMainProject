using System.Collections.Generic;
using UnityEngine;

namespace StatusEffects.ScriptableObjects
{
    [CreateAssetMenu(fileName = "StatusEffectsData", menuName = "GameData/StatusEffects/StatusEffectsData")]
    public class StatusEffectsData : ScriptableObject
    {
        [SerializeField] private EffectInfo[] _buffs;
        [SerializeField] private EffectInfo[] _debuffs;
    
        public IEnumerable<EffectInfo> Buffs => _buffs; 
        public IEnumerable<EffectInfo> Debuffs => _debuffs;
    }
}
