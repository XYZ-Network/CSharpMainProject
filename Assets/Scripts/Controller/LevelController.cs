using System.Linq;
using Assets.Scripts.UnitBrains.Player;
using Model;
using Model.Config;
using Model.Runtime;
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

        private readonly RecommendatedTarget _recommendatedTarget;

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

            RecommendatedTarget recommendatedTarget = new RecommendatedTarget(_runtimeModel, _timeUtil);
            _recommendatedTarget = recommendatedTarget;
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
            
            var unit = new Unit(config, pos, _recommendatedTarget);
            _runtimeModel.Money[forPlayer] -= config.Cost;
            _runtimeModel.PlayersUnits[forPlayer].Add(unit);
        }

        private void TryStartSimulation()
        {
            if (_runtimeModel.Money[RuntimeModel.PlayerId] < _settings.GetCheapestPlayerUnitCost() &&
                _runtimeModel.Money[RuntimeModel.BotPlayerId] < _settings.GetCheapestEnemyUnitCost())
            {
                _runtimeModel.Stage = RuntimeModel.GameStage.Simulation;
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
        }
    }
}