using Model;
using Model.Runtime.Projectiles;
using PlasticPipe.PlasticProtocol.Messages;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Utilities;
using static UnityEngine.GraphicsBuffer;

namespace UnitBrains.Player

{
    public class thirdUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Ironclad Behemoth";
        private const float modeToggleDelay = 1f;
        private float toggleTimer = 0f;
        private bool _attackMode = false;
        private bool _moveMode = true;
        private bool _changeMode = false;

        List<Vector2Int> NextEnemyUnderDistance = new List<Vector2Int>();
        public override Vector2Int GetNextStep()
        {
            if (_attackMode || _changeMode)
            {
                return unit.Pos;
            }
            return base.GetNextStep();
        }

        protected override List<Vector2Int> SelectTargets()
        {
            if (_changeMode || _moveMode)
            {
                return new List<Vector2Int>();
            }
            return base.SelectTargets();
        }

        public override void Update(float deltaTime, float time)
        {
            base.Update(deltaTime, time);
            ChangeMode();
            if (_changeMode)
            {
                toggleTimer += Time.deltaTime;
                float t = toggleTimer/(modeToggleDelay/10);
                if (t >= modeToggleDelay)
                {
                    toggleTimer = 0f;
                    _changeMode = false;
                }
            }
        }
        protected void ChangeMode()
        {
            List<Vector2Int> targets = GetReachableTargets();
            if (targets.Any() && !_attackMode)
            {
                _moveMode = false;
                _changeMode = true;
                _attackMode = true;
            }
            else if (!targets.Any() && !_moveMode)
            {
                _attackMode = false;
                _changeMode = true;
                _moveMode = true;
            }
        }

    }

}
