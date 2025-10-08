using UnityEngine;
using Model.Runtime;
using Model;

namespace Assets.Scripts.Model.Runtime
{
    public interface IBaseAttackDetector
    {
        bool IsAttackingEnemyBase(Unit unit, Vector2Int target);
    }

    public class BaseAttackDetector : IBaseAttackDetector
    {
        private readonly RuntimeModel _runtimeModel;

        public BaseAttackDetector(RuntimeModel runtimeModel)
        {
            _runtimeModel = runtimeModel;
        }

        public bool IsAttackingEnemyBase(Unit unit, Vector2Int target)
        {
            var enemyBaseId = unit.Config.IsPlayerUnit ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId;
            var enemyBasePos = _runtimeModel.Map.Bases[enemyBaseId];
            return target == enemyBasePos;
        }
    }
}