using Model.Runtime.Projectiles;
using System.Collections;
using System.Collections.Generic;
using UnitBrains.Player;
using UnityEngine;

namespace UnitBrains.Player{
    public class ThirdUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Ironclad Behemoth";
        private const float Cooldown = 0.1f;
        private float _cooldownTime = 0f;
        private bool _isCooldown = false;
        private bool _isShoot = false;
        public override void Update(float deltaTime, float time)
        {
            
            if (_isCooldown)
            {
                _cooldownTime += Time.deltaTime;
                if (_cooldownTime >= Cooldown)
                {
                    _cooldownTime = 0f;
                    _isCooldown = false;
                }
            }
                

        }
        public override Vector2Int GetNextStep()
        {
            if (_isCooldown)
                return unit.Pos;
            return base.GetNextStep();
        }
        protected override List<Vector2Int> SelectTargets()
        {
            var result = GetReachableTargets();
            if (result.Count < 1 && _isShoot)
            {
                _isShoot = false;
                _isCooldown = true;
            }
            else if(result.Count > 0 && !_isShoot)
            {
                _isShoot = true;
                _isCooldown = true;

            }
            if (_isCooldown)
                return new List<Vector2Int>();
            while (result.Count > 1)
                result.RemoveAt(result.Count - 1);
            return result;
        }
    }
}