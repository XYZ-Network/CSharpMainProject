using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Model.Runtime;
using Model;
using Model.Runtime;
using UnitBrains;
using UnitBrains.Player;
using UnityEngine;
using Utilities;

namespace Controller
{
    public class SimulationController
    {
        private readonly RuntimeModel _runtimeModel;
        private readonly Action<bool> _onLevelFinished;
        private BuffsController _buffsController;

        public SimulationController(RuntimeModel runtimeModel, Action<bool> onLevelFinished)
        {
            _runtimeModel = runtimeModel;
            _onLevelFinished = onLevelFinished;

            var timeUtil = ServiceLocator.Get<TimeUtil>();

            ServiceLocator.Register<IReadOnlyRuntimeModel>(runtimeModel);
            ServiceLocator.Register<IBaseAttackDetector>(new BaseAttackDetector(runtimeModel));
            _buffsController = new BuffsController();
            ServiceLocator.Register<BuffsController>(_buffsController);

            timeUtil.AddFixedUpdateAction(Update);
        }

        private void Update(float deltaTime)
        {
            if (_runtimeModel.Stage != RuntimeModel.GameStage.Simulation)
                return;

            _buffsController.Update(deltaTime);

            foreach (var unitList in _runtimeModel.PlayersUnits)
                foreach (var unit in unitList.ToList())
                {
                    unit.Update(deltaTime, Time.time);

                    ProcessAttackTargets(unit);

                    _runtimeModel.Projectiles.AddRange(unit.PendingProjectiles);
                    unit.ClearPendingProjectiles();

                    if (unit.Health <= 0)
                    {
                        _buffsController.RemoveBuffs(unit);
                        _runtimeModel.RemoveUnit(unit);
                    }
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

                    _buffsController.AddBuff(hitUnit, new MoveSlowDebuff<Unit>(3f, 0.7f));

                    _buffsController.AddBuff(hitUnit, new AttackSpeedBuff<Unit>(4f, 1.25f));

                    if (hitUnit.Health <= 0)
                    {
                        _buffsController.RemoveBuffs(hitUnit);
                        _runtimeModel.RemoveUnit(hitUnit);
                    }
                }

                for (int i = 0; i < _runtimeModel.Bases.Count; i++)
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

        private void ProcessAttackTargets(Unit unit)
        {
            var targets = unit.GetAttackTargets();
            if (targets == null || targets.Count == 0)
                return;

            foreach (var target in targets)
            {
                _buffsController.CheckAndApplyAttackBaseBuffs(unit, target);
            }
        }

        private void GameOver()
        {
            var isPlayerAlive = _runtimeModel.Bases[RuntimeModel.PlayerId].Health > 0;
            _buffsController.ClearAllBuffs();
            _onLevelFinished?.Invoke(isPlayerAlive);
        }
    }
}