using Model.Runtime.ReadOnly;
using UnitBrains.Pathfinding;
using UnityEngine;

namespace View
{
    public class UnitView : MonoBehaviour
    {
        [SerializeField] private HealthBar _healthBar;
        [SerializeField] private DebugPathOutput _debugPathOutput;

        public void UpdateState(IReadOnlyUnit model, Vector3 prevPosition)
        {
            _healthBar.UpdateHealth((float) model.Health / model.Config.MaxHealth);
            var deltaPos = transform.position - prevPosition;
            if (deltaPos != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(deltaPos, Vector3.up);
            }

            if (_debugPathOutput != null &&
                model.ActivePath != null &&
                model.ActivePath?.EndPoint != _debugPathOutput.Path?.EndPoint)
            {
                _debugPathOutput.HighlightPath(model.ActivePath);
            }
        }
    }
}