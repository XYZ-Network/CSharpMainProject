using Model.Runtime.Projectiles;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UnitBrains.Player
{
    public class ThirdUnitBrain : DefaultPlayerUnitBrain
    {

        public override string TargetUnitName => "Ironclad Behemoth";
        private float switchtime = 0f;
        private bool switchstate = false;
        private int mode = 0;

        public override Vector2Int GetNextStep()
        {
            return switchstate || mode >= 1 ? unit.Pos : base.GetNextStep();
        }

        protected override List<Vector2Int> SelectTargets()
        {
            return switchstate || mode == 0 ? new List<Vector2Int>() : base.SelectTargets();
        }

        public override void Update(float deltaTime, float time)
        {
            int lastmode = mode;
            mode = base.SelectTargets().Count;

            if (lastmode != mode)
                switchstate = true;

            if (switchstate)
            {
                if (switchtime < 1f)
                {
                    switchtime += deltaTime * 10;
                }
                else if (switchtime >= 1f)
                {
                    switchstate = false;
                    switchtime = 0f;
                }

            }
        }
    }

}