using System.Collections.Generic;
using Model.Runtime;
using Model.Runtime.ReadOnly;
using UnityEngine;

namespace Model
{
    public interface IReadOnlyRuntimeModel
    {
        IReadOnlyMap RoMap { get; } // карта которая доступна для чтения
        RuntimeModel.GameStage Stage { get; } // текщее состояние игры
        public int Level { get; } // уровень
        public IReadOnlyDictionary<int, int> RoMoney { get; } // деньги
        public IEnumerable<IReadOnlyUnit> RoUnits { get; } // все юниты
        public IEnumerable<IReadOnlyProjectile> RoProjectiles { get; } // все тайлы        
        public IEnumerable<IReadOnlyUnit> RoPlayerUnits { get; } // юниты игрока
        public IEnumerable<IReadOnlyUnit> RoBotUnits { get; } // юниты врага
        public IReadOnlyList<IReadOnlyBase> RoBases { get; } // все базы

        public bool IsTileWalkable(Vector2Int pos); // является ли ячейка проходимой
    }
}