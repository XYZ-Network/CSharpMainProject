using System.Collections;
using System.Collections.Generic;
using UnitBrains.Player;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace UnitBrains.Player
{
    public class ThirdUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Ironclad Behemoth";

        private const float TransitionDuration = 1f;
        private bool _isShooting = false;
        private bool isInTransition = false;
        private bool _hasTargets = false;
        private float transitionStartTime = 0f;
        public override void Update(float deltaTime, float time)
        {
            base.Update(deltaTime, time);

            if (_isShooting && !_hasTargets && !isInTransition ||
                !_isShooting && _hasTargets && !isInTransition)
            {
                isInTransition = true;
                transitionStartTime = time;
            }

            if (isInTransition && time - transitionStartTime >= TransitionDuration)
            {
                _isShooting = !_isShooting;
                isInTransition = false;
            }
        }


        protected override List<Vector2Int> SelectTargets()
        {
            var result = base.SelectTargets();
            _hasTargets = result.Count > 0;
            if (!_isShooting || isInTransition)
                result.Clear();

            return result;
        }
    }
}
