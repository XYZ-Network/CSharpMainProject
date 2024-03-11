using System.Collections.Generic;
using Model.Runtime;
using Model.Runtime.ReadOnly;
using UnityEngine;

namespace Model
{
    public interface IReadOnlyRuntimeModel
    {
        IReadOnlyMap RoMap { get; } //карта, доступная только для чтения
        RuntimeModel.GameStage Stage { get; } //текущее состояние игры
        public int Level { get; } //уровень
        public IReadOnlyDictionary<int, int> RoMoney { get; } //количество денег
        public IEnumerable<IReadOnlyUnit> RoUnits { get; } //все юниты
        public IEnumerable<IReadOnlyProjectile> RoProjectiles { get; } //все проджектайлы
        
        public IEnumerable<IReadOnlyUnit> RoPlayerUnits { get; } //все юниты принадлежащие игроку
        public IEnumerable<IReadOnlyUnit> RoBotUnits { get; } //все юниты принадлежащие боту
        public IReadOnlyList<IReadOnlyBase> RoBases { get; } //все базы

        public bool IsTileWalkable(Vector2Int pos); //явлияется ли клетка карты проходимой
    }
}