using Assets.Scripts.Model.Runtime.Effects;
using Model;
using Model.Runtime;
using Model.Runtime.Projectiles;
using System.Collections.Generic;
using System.Linq;
using UnitBrains.Player;
using UnityEngine;
using Utilities;
using View;
using Random = System.Random;

namespace Assets.Scripts.UnitBrains.Player
{
    public class SkyBufferUnitBrain : DefaultPlayerUnitBrain
    {
        private readonly Random _random = new Random();
        private VFXView _vfxView = ServiceLocator.Get<VFXView>();

        private const float _changingTime = 0.5f;

        public override string TargetUnitName => "Sky Buffer";
        private float _timeBeforeChange = 0f;
        private EffectManager _effectManager;

        public SkyBufferUnitBrain()
        {
            _effectManager = ServiceLocator.Get<EffectManager>();
        }

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            if (_timeBeforeChange != 0)
            {
                return;
            }

            var unitToBuff = runtimeModel.RoUnits
                .Where(u => u.Pos == forTarget)
                .FirstOrDefault();

            var effects = _effectManager.GetEffectsOnTarget(unitToBuff);

            var availableEffectsToAdd = Effect.AvailableBuffEffects.Except(effects.Select(e => e.GetType()))
                .ToList();

            if (availableEffectsToAdd.Any())
            {
                var index = _random.Next(availableEffectsToAdd.Count - 1);

                _effectManager.AddEffect(unitToBuff as Unit, availableEffectsToAdd[index]);

                _vfxView.PlayVFX(unitToBuff.Pos, VFXView.VFXType.BuffApplied);

                _timeBeforeChange = _changingTime;

                Debug.Log("Effect applied");
            }
        }

        public override Vector2Int GetNextStep()
        {
            var nextStep = base.GetNextStep();

            if (_timeBeforeChange > 0)
            {
                return unit.Pos;
            }

            return nextStep;
        }

        protected override List<Vector2Int> SelectTargets()
        {
            var targets = SelectAllyTargets();

            if (_timeBeforeChange > 0)
            {
                return new List<Vector2Int>();
            }

            return targets;
        }

        public override void Update(float deltaTime, float time)
        {
            base.Update(deltaTime, time);

            if (_timeBeforeChange > 0)
            {
                _timeBeforeChange -= deltaTime;
            }

            if (_timeBeforeChange < 0)
            {
                _timeBeforeChange = 0;
            }
        }

        private IEnumerable<Vector2Int> GetAllAllyTargets()
        {
            return runtimeModel.RoUnits
                .Where(u => u.Config.IsPlayerUnit == IsPlayerUnitBrain)
                .Select(u => u.Pos);
        }

        protected List<Vector2Int> GetReachableAllyTargets()
        {
            var result = new List<Vector2Int>();
            var attackRangeSqr = unit.Config.AttackRange * unit.Config.AttackRange;
            foreach (var possibleTarget in GetAllAllyTargets())
            {
                if (!IsTargetInRange(possibleTarget))
                    continue;

                result.Add(possibleTarget);
            }

            return result;
        }

        protected virtual List<Vector2Int> SelectAllyTargets()
        {
            var result = GetReachableAllyTargets();
            while (result.Count > 1)
                result.RemoveAt(result.Count - 1);
            return result;
        }
    }
}