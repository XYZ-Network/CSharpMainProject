
using System;
using System.Linq;
using Model;
using UnityEngine;
using Utilities;

namespace Controller
{
    public class SimulationController
    {
        private readonly RuntimeModel _runtimeModel;
        private readonly Action<bool> _onLevelFinished;

        public SimulationController(RuntimeModel runtimeModel, Action<bool> onLevelFinished)
        {
            _runtimeModel = runtimeModel;
            _onLevelFinished = onLevelFinished;
            
            var timeUtil = ServiceLocator.Get<TimeUtil>();
            
            timeUtil.AddFixedUpdateAction(Update);
        }
        
        private void Update(float deltaTime)
        {
            if (_runtimeModel.Stage != RuntimeModel.GameStage.Simulation)
                return;

            foreach (var unitList in _runtimeModel.PlayersUnits)
                foreach (var unit in unitList)
                {
                    unit.Update(deltaTime, Time.time);
                    _runtimeModel.Projectiles.AddRange(unit.PendingProjectiles);
                    unit.ClearPendingProjectiles();
                }

            foreach (var projectile in _runtimeModel.Projectiles)
            {
                projectile.Update(deltaTime, Time.time);
                if (!projectile.HadHit)
                    continue;
                
                var hitUnit = _runtimeModel.AllUnits.FirstOrDefault(u => u.Pos == projectile.HitTile);
                if (hitUnit != null)
                {
                    hitUnit.TakeDamage(projectile.Damage);
                    if (hitUnit.Health <= 0)
                    {
                        _runtimeModel.RemoveUnit(hitUnit);
                    }
                }
                
                for (int i=0; i<_runtimeModel.Bases.Count; i++)
                {
                    var pos = _runtimeModel.Map.Bases[i];
                    if (pos != projectile.HitTile)
                        continue;
                    
                    var playerBase = _runtimeModel.Bases[i];
                    playerBase.TakeDamage(projectile.Damage);
                    if (playerBase.Health <= 0)
                    {
                        GameOver();
                    }
                }
            }
                
            _runtimeModel.Projectiles.RemoveAll(p => p.HadHit);
        }

        private void GameOver()
        {
            var isPlayerAlive = _runtimeModel.Bases[RuntimeModel.PlayerId].Health > 0;
            _onLevelFinished?.Invoke(isPlayerAlive);
        }
    }
}