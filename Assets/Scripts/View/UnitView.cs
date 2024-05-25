using Model.Runtime.ReadOnly;
using UnityEngine;

namespace View
{
    public class UnitView : MonoBehaviour
    {
        [SerializeField] private HealthBar _healthBar;

        public void UpdateState(IReadOnlyUnit model, Vector3 prevPosition)
        {
            _healthBar.UpdateHealth((float) model.Health / model.Config.MaxHealth);
            var deltaPos = transform.position - prevPosition;
            if (deltaPos != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(deltaPos, Vector3.up);
            }
        }
    }
}