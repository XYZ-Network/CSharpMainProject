using System.Linq;
using Buffs;
using Model;
using Model.Config;
using Model.Runtime;
using UnitBrains;
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

        private readonly PlayerCoordinator _playerCoordinator;
        private readonly PlayerCoordinator _botCoordinator;
        private readonly BuffSystem _buffSystem;

        public LevelController(RuntimeModel runtimeModel, RootController rootController)
        {
            _runtimeModel = runtimeModel;
            _rootController = rootController;

            _rootView = ServiceLocator.Get<RootView>();
            _gameplayView = ServiceLocator.Get<Gameplay3dView>();
            _settings = ServiceLocator.Get<Settings>();
            _timeUtil = ServiceLocator.Get<TimeUtil>();

            _botController = new BotController(OnBotUnitChosen);

            _buffSystem = new BuffSystem(_timeUtil);
            ServiceLocator.Register(_buffSystem);

            _simulationController = new SimulationController(runtimeModel, OnLevelFinished);

            _playerCoordinator = new PlayerCoordinator(
                _runtimeModel,
                _timeUtil,
                RuntimeModel.PlayerId,
                RuntimeModel.BotPlayerId);

            _botCoordinator = new PlayerCoordinator(
                _runtimeModel,
                _timeUtil,
                RuntimeModel.BotPlayerId,
                RuntimeModel.PlayerId);

            UnitBrainProvider.SetCoordinator(RuntimeModel.PlayerId, _playerCoordinator);
            UnitBrainProvider.SetCoordinator(RuntimeModel.BotPlayerId, _botCoordinator);
        }

        public void StartLevel(int level)
        {
            ServiceLocator.RegisterAs(this, typeof(IPlayerUnitChoosingListener));

            _rootView.HideLevelFinished();

            Random.InitState(level);
            SetInitialMoney();

            var density = Random.Range(_settings.MapMinDensity, _settings.MapMaxDensity);

            var map = MapGenerator.Generate(
                _settings.MapWidth,
                _settings.MapHeight,
                density,
                level);

            _runtimeModel.Clear();
            _buffSystem.Clear();

            _runtimeModel.Map = new Map(map, Settings.PlayersCount);
            _runtimeModel.Stage = RuntimeModel.GameStage.ChooseUnit;

            _runtimeModel.Bases[RuntimeModel.PlayerId] = new MainBase(_settings.MainBaseMaxHp);
            _runtimeModel.Bases[RuntimeModel.BotPlayerId] = new MainBase(_settings.MainBaseMaxHp);

            _gameplayView.Reinitialize();
        }

        public void OnPlayersUnitChosen(UnitConfig unitConfig)
        {
            if (_runtimeModel.Stage != RuntimeModel.GameStage.ChooseUnit)
                return;

            if (unitConfig.Cost > _runtimeModel.Money[RuntimeModel.PlayerId])
                return;

            SpawnUnit(RuntimeModel.PlayerId, unitConfig);

            _botController.ChooseUnit();
            TryStartSimulation();
        }

        private void OnBotUnitChosen(UnitConfig unitConfig)
        {
            if (_runtimeModel.Stage != RuntimeModel.GameStage.ChooseUnit)
                return;

            if (unitConfig.Cost > _runtimeModel.Money[RuntimeModel.BotPlayerId])
                return;

            SpawnUnit(RuntimeModel.BotPlayerId, unitConfig);
            TryStartSimulation();
        }

        private void SpawnUnit(int forPlayer, UnitConfig config)
        {
            var pos = _runtimeModel.Map.FindFreeCellNear(
                _runtimeModel.Map.Bases[forPlayer],
                _runtimeModel.RoUnits.Select(x => x.Pos).ToHashSet());

            var unit = new Unit(config, pos);

            _runtimeModel.Money[forPlayer] -= config.Cost;
            _runtimeModel.PlayersUnits[forPlayer].Add(unit);
        }

        private void TryStartSimulation()
        {
            if (_runtimeModel.Stage != RuntimeModel.GameStage.ChooseUnit)
                return;

            int cheapestPlayerUnitCost = _settings.PlayerUnits.Keys.Min(unit => unit.Cost);
            int cheapestEnemyUnitCost = _settings.EnemyUnits.Keys.Min(unit => unit.Cost);

            bool playerCanBuyMore = _runtimeModel.Money[RuntimeModel.PlayerId] >= cheapestPlayerUnitCost;
            bool botCanBuyMore = _runtimeModel.Money[RuntimeModel.BotPlayerId] >= cheapestEnemyUnitCost;

            bool playerHasUnits = _runtimeModel.PlayersUnits[RuntimeModel.PlayerId].Count > 0;
            bool botHasUnits = _runtimeModel.PlayersUnits[RuntimeModel.BotPlayerId].Count > 0;

            Debug.Log(
                $"TryStartSimulation | " +
                $"PlayerMoney={_runtimeModel.Money[RuntimeModel.PlayerId]} | " +
                $"BotMoney={_runtimeModel.Money[RuntimeModel.BotPlayerId]} | " +
                $"CheapestPlayer={cheapestPlayerUnitCost} | " +
                $"CheapestBot={cheapestEnemyUnitCost} | " +
                $"PlayerCanBuy={playerCanBuyMore} | " +
                $"BotCanBuy={botCanBuyMore} | " +
                $"PlayerHasUnits={playerHasUnits} | " +
                $"BotHasUnits={botHasUnits}");

            if (!playerHasUnits && !playerCanBuyMore)
            {
                Debug.Log("PLAYER HAS NO UNITS AND CANNOT BUY MORE -> LOSE");
                OnLevelFinished(false);
                return;
            }

            if (!botHasUnits && !botCanBuyMore)
            {
                Debug.Log("BOT HAS NO UNITS AND CANNOT BUY MORE -> WIN");
                OnLevelFinished(true);
                return;
            }

            if (playerCanBuyMore || botCanBuyMore)
                return;

            if (!playerHasUnits || !botHasUnits)
                return;

            Debug.Log("START SIMULATION");
            _runtimeModel.Stage = RuntimeModel.GameStage.Simulation;
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