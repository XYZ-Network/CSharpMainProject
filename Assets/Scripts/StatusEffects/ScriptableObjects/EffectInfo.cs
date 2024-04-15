using UnityEngine;

namespace StatusEffects.ScriptableObjects
{
    [CreateAssetMenu(fileName = "EffectInfo", menuName = "GameData/StatusEffects/EffectInfo")]
    public class EffectInfo : ScriptableObject
    {
        [SerializeField] private Attributes _attribute;
        [SerializeField] private bool _isBuff;
        [SerializeField][Range(0.01f, 5f)] private float _modifier;
        [SerializeField][Range(0.01f, 15f)]  private float _duration;

        public Attributes Attribute  => _attribute;
        public bool IsBuff => _isBuff;
        public float Modifier => _modifier;
        public float Duration => _duration;
    }
}
