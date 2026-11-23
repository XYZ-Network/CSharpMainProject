using System.Linq;
using Model;
using Model.Config;
using Model.Runtime;
using Model.Runtime.Buffs;
using UnitBrains.Player;
using UnityEngine;
using Utilities;
using View;

namespace Controller
{
    public class LevelController : IPlayerUnitChoosingListener
    {
        private readonly RuntimeModel _runtimeModel;
        private readonly RootController _rootController;
        private readonly BotController _botController;
        private readonly SimulationController _simulationController;
        private readonly RootView _rootView;
        private readonly Gameplay3dView _gameplayView;
        private readonly Settings _settings;
        private readonly TimeUtil _timeUtil;
        private readonly BuffSystem _buffSystem;

        public LevelController(RuntimeModel runtimeModel, RootController rootController)
        {
            _runtimeModel = runtimeModel;
            _rootController = rootController;
            _botController = new BotController(OnBotUnitChosen);
            _simulationController = new(runtimeModel, OnLevelFinished);
            
            _rootView = ServiceLocator.Get<RootView>();
            _gameplayView = ServiceLocator.Get<Gameplay3dView>();
            _settings = ServiceLocator.Get<Settings>();
            _timeUtil = ServiceLocator.Get<TimeUtil>();
            _buffSystem = ServiceLocator.Get<BuffSystem>();
        }

        public void StartLevel(int level)
        {
            ServiceLocator.RegisterAs(this, typeof(IPlayerUnitChoosingListener));
            
            _rootView.HideLevelFinished();

            Random.InitState(level);
            SetInitialMoney();
            var density = Random.Range(_settings.MapMinDensity, _settings.MapMaxDensity);
            var map = MapGenerator.Generate(_settings.MapWidth, _settings.MapHeight, density, level);
            _runtimeModel.Clear();
            _runtimeModel.Map = new Map(map, Settings.PlayersCount);
            _runtimeModel.Stage = RuntimeModel.GameStage.ChooseUnit;
            _runtimeModel.Bases[RuntimeModel.PlayerId] = new MainBase(_settings.MainBaseMaxHp);
            _runtimeModel.Bases[RuntimeModel.BotPlayerId] = new MainBase(_settings.MainBaseMaxHp);

            _gameplayView.Reinitialize();
        }

        public void OnPlayersUnitChosen(UnitConfig unitConfig)
        {
            if (unitConfig.Cost > _runtimeModel.Money[RuntimeModel.PlayerId])
                return;
            
            SpawnUnit(RuntimeModel.PlayerId, unitConfig);
            TryStartSimulation();
        }

        private void OnBotUnitChosen(UnitConfig unitConfig)
        {
            SpawnUnit(RuntimeModel.BotPlayerId, unitConfig);
            TryStartSimulation();
        }

        private void SpawnUnit(int forPlayer, UnitConfig config)
        {
            var pos = _runtimeModel.Map.FindFreeCellNear(
                _runtimeModel.Map.Bases[forPlayer],
                _runtimeModel.RoUnits.Select(x => x.Pos).ToHashSet());
            
            var coordinator = new UnitCoordinator(_runtimeModel, _timeUtil);
            var unit = new Unit(config, coordinator, pos);
            _runtimeModel.Money[forPlayer] -= config.Cost;
            _runtimeModel.PlayersUnits[forPlayer].Add(unit);
        }

        private void TryStartSimulation()
        {
            if (_runtimeModel.Money[RuntimeModel.PlayerId] < _settings.GetCheapestPlayerUnitCost() &&
                _runtimeModel.Money[RuntimeModel.BotPlayerId] < _settings.GetCheapestEnemyUnitCost())
            {
                _runtimeModel.Stage = RuntimeModel.GameStage.Simulation;
                
                // Buff System Test Block - buff all player units at start
                // add debuff (decrease Attack and Move delay x2 times)
                // add buff (invulnerability)
                // for all player units for 5 sec at start of battle
                // foreach (var u in _runtimeModel.PlayersUnits[RuntimeModel.PlayerId])
                // {
                //     _buffSystem.AddBuffToUnit(u, new Buff(Buff.BuffType.AttackSpeed, 5, -2));
                //     _buffSystem.AddBuffToUnit(u, new Buff(Buff.BuffType.MoveSpeed, 5, -2));
                //     _buffSystem.AddBuffToUnit(u, new Buff(Buff.BuffType.Invulnerability, 5, 1));
                // }
            }
        }

        private void SetInitialMoney()
        {
            var startMoney = _settings.BaseLevelMoney + _runtimeModel.Level * _settings.LevelMoneyIncrement;
            var botMoneyAdvantage = (_runtimeModel.Level + _settings.BotMoneyAdvantageLevelShift) *
                                    _settings.BotMoneyAdvantagePerLevel;
            _runtimeModel.SetMoneyForAll(startMoney, startMoney + botMoneyAdvantage);
        }

        private void OnLevelFinished(bool playerWon)
        {
            _runtimeModel.Stage = RuntimeModel.GameStage.Finished;
            _rootView.ShowLevelFinished(playerWon);
            _timeUtil.RunDelayed(5f, () => _rootController.OnLevelFinished(playerWon));
            _buffSystem.Clear();
        }
    }
}