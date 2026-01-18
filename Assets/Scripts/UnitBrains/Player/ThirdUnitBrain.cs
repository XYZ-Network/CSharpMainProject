using Model;
using Model.Runtime.Projectiles;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Utilities;

namespace UnitBrains.Player
{
    public class ThirdUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Ironclad Behemoth";
        private enum UnitStates
        {
            Moving,
            Attacking,
            Transition
        }
        
        private float currentTransitionTime = 0f;
        private float transitionTime;
        private List<Vector2Int> TargetOutOfRange = new List<Vector2Int>();
        private UnitStates currentState = UnitStates.Moving;
        private UnitStates nextState = UnitStates.Moving;

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            if (currentState == UnitStates.Attacking)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }
        }

        public override void Update(float deltaTime, float time)
        {
            currentTransitionTime = 0f;
            if (currentState == UnitStates.Transition)
            {
                while (currentTransitionTime < transitionTime)
                {
                    currentTransitionTime += deltaTime;
                }
                currentState = nextState;
            }
        }

        protected override List<Vector2Int> SelectTargets()
        {
            List<Vector2Int> result = new List<Vector2Int>();
            TargetOutOfRange.Clear();
            IEnumerable<Vector2Int> resultAsIE = GetAllTargets();
            result = resultAsIE.ToList();
            Vector2Int enemyBase = runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId];
            if (!result.Any())
                result.Add(enemyBase);
            SortByDistanceToOwnBase(result);

            Vector2Int enemy = new Vector2Int(0, 0);

            while (result.Count != 1)
            {
                result.RemoveAt(result.Count - 1);
            }

            enemy = result[0];
            result.Clear();
            if (IsTargetInRange(enemy))
            {
                if (currentState != UnitStates.Attacking)
                {
                    nextState = UnitStates.Attacking;
                    currentState = UnitStates.Transition;
                }
                else
                {
                    result.Add(enemy);
                }
            }
            else
            {
                if (currentState != UnitStates.Moving)
                {
                    nextState = UnitStates.Moving;
                    currentState = UnitStates.Transition;
                }
                else
                {
                    TargetOutOfRange.Add(enemy);
                }
            }
            return result;
        }

        public override Vector2Int GetNextStep()
        {
            Vector2Int position = unit.Pos;
            if ((currentState == UnitStates.Moving) && TargetOutOfRange.Any())
            {
                foreach (var target in TargetOutOfRange)
                {
                    position = unit.Pos;
                    position = position.CalcNextStepTowards(target);
                }
                return position;
            }
            else
            {
                return unit.Pos;
            }
        }

        public float TransitionTime { get; private set; } = 1f;
    }
}