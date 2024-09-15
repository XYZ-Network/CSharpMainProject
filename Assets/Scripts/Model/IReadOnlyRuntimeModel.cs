using System.Collections.Generic;
using Model.Runtime;
using Model.Runtime.ReadOnly;
using UnityEngine;

namespace Model
{
    public interface IReadOnlyRuntimeModel
    {
        IReadOnlyMap RoMap { get; } // ссылка на базу
        RuntimeModel.GameStage Stage { get; }
        public int Level { get; }
        public IReadOnlyDictionary<int, int> RoMoney { get; }
        public IEnumerable<IReadOnlyUnit> RoUnits { get; }
        public IEnumerable<IReadOnlyProjectile> RoProjectiles { get; }
        
        public IEnumerable<IReadOnlyUnit> RoPlayerUnits { get; } // позиция наших юнитов
        public IEnumerable<IReadOnlyUnit> RoBotUnits { get; } // позиция вражеского юнитов
        public IReadOnlyList<IReadOnlyBase> RoBases { get; }

        public bool IsTileWalkable(Vector2Int pos);
    }
}