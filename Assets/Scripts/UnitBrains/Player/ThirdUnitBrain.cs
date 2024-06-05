using System;
using System.Collections.Generic;
using Model;
using Model.Runtime.Projectiles;
using Unity.VisualScripting;
using UnityEngine;
using Utilities;

namespace UnitBrains.Player
{
    public class ThirdUnitBrain : DefaultPlayerUnitBrain
    {
        private enum UnitState
        {
            Move,
            Attack
        }

        public override string TargetUnitName => "Ironclad Behemoth";
        private UnitState currentState = UnitState.Move;
        private float transitionTime = 1f;
        private bool stateChange = true;

        //protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        //{            
        //    var projectile = CreateProjectile(forTarget);
        //    AddProjectileToList(projectile, intoList);
        //}

        public override Vector2Int GetNextStep()
        {
            Vector2Int position = base.GetNextStep();
            bool checkTrue = (position == unit.Pos);

            if (checkTrue)
                currentState = UnitState.Attack;
            else
                currentState = UnitState.Move;

            stateChange = checkTrue;
            return stateChange ? unit.Pos : position;
        }

        public override void Update(float deltaTime, float time)
        {
            if (stateChange)
            {
                transitionTime -= deltaTime;

                if (transitionTime <= 0)
                {
                    transitionTime = 1f;
                    stateChange = false;
                }
            }
            base.Update(deltaTime, time);
        }
        protected override List<Vector2Int> SelectTargets()
        {
            if (stateChange)
                return new List<Vector2Int>();
            if (currentState == UnitState.Attack)
                return base.SelectTargets();
            return new List<Vector2Int>();
        }

    }
}