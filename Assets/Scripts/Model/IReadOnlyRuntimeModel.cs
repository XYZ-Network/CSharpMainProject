using System.Collections.Generic;
using Model.Runtime;
using Model.Runtime.ReadOnly;
using UnityEngine;

namespace Model
{
    public interface IReadOnlyRuntimeModel
    {
        IReadOnlyMap RoMap { get; }
        RuntimeModel.GameStage Stage { get; }
        public int Level { get; }
        public IReadOnlyDictionary<int, int> RoMoney { get; }
        public IEnumerable<IReadOnlyUnit> RoUnits { get; }
        public IEnumerable<IReadOnlyProjectile> RoProjectiles { get; }
        
        public IEnumerable<IReadOnlyUnit> RoPlayerUnits { get; }
        public IEnumerable<IReadOnlyUnit> RoBotUnits { get; }
        public IReadOnlyList<IReadOnlyBase> RoBases { get; }

        public bool IsTileWalkable(Vector2Int pos);
    }
}