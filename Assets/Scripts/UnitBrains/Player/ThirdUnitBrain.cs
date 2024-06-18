using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitBrains.Player
{
    enum ThirdUnitBrainAction
    {
        Any,
        Attack,
        Move
    };
    /**
     *  Ironclad Bahemoth может либо ехать, либо стрелять. 
     *  Между этими двумя режимами он переключается 1 секунду, во время переключения он ничего не делает, не стреляет, не едет.
     *  При этом у него высокая скорость атаки 0.15. 
     */
    public class ThirdUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Ironclad Behemoth";

        private ThirdUnitBrainAction AllowedAction = ThirdUnitBrainAction.Any;
        private float lastActionTime = 0f;

        public override void Update(float deltaTime, float time)
        {
            if (AllowedAction != ThirdUnitBrainAction.Any)
            {
                if (time - lastActionTime >= 1)
                {
                    AllowedAction = ThirdUnitBrainAction.Any;
                }
            }
        }

        /**
         * Отвечает за выбор ячейки для передвижения
         */
        public override Vector2Int GetNextStep()
        {
            Debug.Log("GetNextStep");
            Vector2Int nextStepForMove = base.GetNextStep();
            if (AllowedAction == ThirdUnitBrainAction.Any && !Vector2Int.Equals(nextStepForMove, unit.Pos))
            {
                AllowedAction = ThirdUnitBrainAction.Move;
            }

            if (AllowedAction == ThirdUnitBrainAction.Move && !Vector2Int.Equals(nextStepForMove, unit.Pos))
            {
                lastActionTime = Time.time;
            }

            return AllowedAction == ThirdUnitBrainAction.Move ? nextStepForMove : unit.Pos;
        }

        /**
         * Отвечает за выбор целей для атаки
         */
        protected override List<Vector2Int> SelectTargets()
        {
            Debug.Log("SelectTargets");
            List <Vector2Int> targets = base.SelectTargets();
            if (AllowedAction == ThirdUnitBrainAction.Any && targets.Count > 0)
            {
                AllowedAction = ThirdUnitBrainAction.Attack;
            }

            if (AllowedAction == ThirdUnitBrainAction.Attack && targets.Count > 0)
            {
                lastActionTime = Time.time;
            }

            return AllowedAction == ThirdUnitBrainAction.Attack ? targets : new List<Vector2Int>();
        }
    }
}

