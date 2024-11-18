using System.Collections.Generic;
using System.Linq;
using Model.Runtime;
using Model.Runtime.Projectiles;
using Model.Runtime.ReadOnly;
using UnityEngine;

namespace Model
{
    public class RuntimeModel : IReadOnlyRuntimeModel
    {
        public const int PlayerId = 0;
        public const int BotPlayerId = 1;
        
        public IReadOnlyMap RoMap => Map;
        public IReadOnlyDictionary<int, int> RoMoney => Money;

        public IEnumerable<IReadOnlyUnit> RoUnits =>
            _playersUnits[PlayerId].Concat(_playersUnits[BotPlayerId]);
        public IEnumerable<IReadOnlyProjectile> RoProjectiles => Projectiles;
        
        public IEnumerable<IReadOnlyUnit> RoPlayerUnits => _playersUnits[PlayerId];
        public IEnumerable<IReadOnlyUnit> RoBotUnits => _playersUnits[BotPlayerId];
        public IReadOnlyList<IReadOnlyBase> RoBases => Bases;
        
        public Map Map { get; set; }
        public GameStage Stage { get; set; } = GameStage.None;
        public int Level { get; set; }
        public IReadOnlyList<List<Unit>> PlayersUnits => _playersUnits;
        public IEnumerable<Unit> AllUnits => _playersUnits.SelectMany(l => l);
        public List<BaseProjectile> Projectiles { get; } = new();
        public List<MainBase> Bases { get; } = new()
        {
            null, null
        };

        public Dictionary<int, int> Money { get; } = new()
        {
            { PlayerId, 0 },
            { BotPlayerId, 0 }
        };
        
        private readonly List<List<Unit>> _playersUnits = new()
        {
            new (), new ()
        };
        
        public bool IsTileWalkable(Vector2Int pos)
        {
            return !Map[pos] && AllUnits.All(u => u.Pos != pos);
        }

        public void SetMoneyForAll(int startMoneyPlayer, int startMoneyBot)
        {
            Money[PlayerId] = startMoneyPlayer;
            Money[BotPlayerId] = startMoneyBot;
        }

        public void RemoveUnit(Unit hitUnit)
        {
            foreach (var list in _playersUnits)
            {
                list.Remove(hitUnit);
            }
        }

        public void Clear()
        {
            foreach (var list in _playersUnits)
                list.Clear();
            
            Projectiles.Clear();
        }
        
        public enum GameStage
        {
            None,
            ChooseUnit,
            Simulation,
            Finished,
        }
    }
}