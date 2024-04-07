using UnityEngine;

namespace View
{
    public class ProjectileView : MonoBehaviour
    {
        [SerializeField] private string _projTypeName;
        
        public string ProjTypeName => _projTypeName;
    }
}