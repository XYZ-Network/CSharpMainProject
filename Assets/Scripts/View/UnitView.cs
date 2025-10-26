using Assets.Scripts.UnitBrains;
using Model;
using Model.Runtime.ReadOnly;
using System.Linq;
using UnitBrains;
using UnitBrains.Pathfinding;
using UnityEngine;
using Utilities;

namespace View
{
    public class UnitView : MonoBehaviour
    {
        [SerializeField] private HealthBar _healthBar;
        [SerializeField] private DebugPathOutput _debugPathOutput;
        private Vector2Int[] _lastPathCells;
        public void UpdateState(IReadOnlyUnit model, Vector3 prevPosition)
        {
            _healthBar.UpdateHealth((float) model.Health / model.Config.MaxHealth);
            var deltaPos = transform.position - prevPosition;
            if (deltaPos != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(deltaPos, Vector3.up);
            }

#if UNITY_EDITOR
            if (_debugPathOutput != null)
            {
                int enemyPlayerId = model.Config.IsPlayerUnit
                    ? RuntimeModel.BotPlayerId
                    : RuntimeModel.PlayerId;

                var runtimeModel = ServiceLocator.Get<IReadOnlyRuntimeModel>();
                Vector2Int target = runtimeModel.RoMap.Bases[enemyPlayerId];
                Vector2Int startBase = runtimeModel.RoMap.Bases[
                model.Config.IsPlayerUnit ? RuntimeModel.PlayerId : RuntimeModel.BotPlayerId];
                Vector2Int endBase = runtimeModel.RoMap.Bases[
                    model.Config.IsPlayerUnit ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId
                ];

                var tempPath = SmartMoveUnitBrain.GetPath(runtimeModel, startBase, endBase, model.Pos);
                var currentPathCells = tempPath.GetPath().ToArray();

                
                if (!PathsEqual(_lastPathCells, currentPathCells))
                {
                    _debugPathOutput.HighlightPath(tempPath);
                    _lastPathCells = currentPathCells;
                }
            }
#endif
        }
        private bool PathsEqual(Vector2Int[] a, Vector2Int[] b)
        {
            if (a == null || b == null) return a == b;
            if (a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i]) return false;
            }
            return true;
        }
    }
}
