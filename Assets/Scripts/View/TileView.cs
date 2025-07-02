using UnityEngine;
using UnityEngine.Serialization;

namespace View
{
    public class TileView : MonoBehaviour
    {
        [SerializeField] private bool isBlocked;
        [SerializeField] private bool isBasePlayer;
        [SerializeField] private bool isBaseEnemy;
        
        public bool IsBlocked => isBlocked;
        public bool IsBasePlayer => isBasePlayer;
        public bool IsBaseEnemy => isBaseEnemy;

        private void Start()
        {
            transform.rotation = Quaternion.Euler(0, Random.Range(0, 4) * 90, 0);
        }
    }
}