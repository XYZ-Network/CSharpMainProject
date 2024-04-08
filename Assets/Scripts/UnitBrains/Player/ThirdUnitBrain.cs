using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitBrains.Player;
using UnityEngine;

namespace Assets.Scripts.UnitBrains.Player
{
    internal class ThirdUnitBrain : DefaultPlayerUnitBrain
    {
        private enum BehemothPreviousAction { Move, Attack}
        private BehemothPreviousAction _prevAction = BehemothPreviousAction.Move;
        private bool _isOnCooldown = false;
        private float _cooldownTimer = 0;
        private const float COOLDOWN_TIME = 0.1f;
        public override string TargetUnitName => "Ironclad Behemoth";

        public override void Update(float deltaTime, float time)
        {
            if (_isOnCooldown)
            {
                if(_cooldownTimer < COOLDOWN_TIME)
                {
                    _cooldownTimer += Time.deltaTime;
                }
                else
                {
                    _cooldownTimer = 0;
                    _isOnCooldown = false;
                }
            }
        }

        protected override List<Vector2Int> SelectTargets()
        {
            if (_isOnCooldown)
                return new List<Vector2Int>();

            var result = GetReachableTargets();

            if(HasTargetsInRange() && _prevAction == BehemothPreviousAction.Move)
            {
                _prevAction = BehemothPreviousAction.Attack;
                _isOnCooldown = true;
                return new List<Vector2Int>();
            }
                    
            while (result.Count > 1)
                result.RemoveAt(result.Count - 1);
            if (result.Count > 0)
                _prevAction = BehemothPreviousAction.Attack;
            return result;

        }

        public override Vector2Int GetNextStep()
        {
            if(_prevAction == BehemothPreviousAction.Attack && !HasTargetsInRange())
            {
                _prevAction = BehemothPreviousAction.Move;
                _isOnCooldown = true;
            }

            if (_isOnCooldown || HasTargetsInRange())
                return unit.Pos;

            var target = runtimeModel.RoMap.Bases[
                IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId];

            _prevAction = BehemothPreviousAction.Move;
            return CalcNextStepTowards(target);
        }
    }
}
