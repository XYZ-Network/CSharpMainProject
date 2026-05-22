using System;
using System.Collections.Generic;
using System.Linq;
using Model;
using Model.Config;
using Utilities;

namespace Controller
{
    public class BotController
    {
        private readonly IReadOnlyRuntimeModel _runtimeModel;
        private readonly List<UnitConfig> _sortedUnits;
        private readonly Action<UnitConfig> _onBotUnitChosen;

        public BotController(Action<UnitConfig> onBotUnitChosen)
        {
            _onBotUnitChosen = onBotUnitChosen;
            _runtimeModel = ServiceLocator.Get<IReadOnlyRuntimeModel>();
            _sortedUnits = ServiceLocator.Get<Settings>()
                .EnemyUnits.Keys
                .OrderBy(x => x.Cost)
                .ToList();
        }

        public void Stop()
        {
        }

        public void ChooseUnit()
        {
            if (_runtimeModel.Stage != RuntimeModel.GameStage.ChooseUnit)
                return;

            if (_sortedUnits.Count == 0)
                return;

            var moneyLeft = _runtimeModel.RoMoney[RuntimeModel.BotPlayerId];

            for (int i = _sortedUnits.Count - 1; i >= 0; i--)
            {
                var unit = _sortedUnits[i];
                if (unit.Cost <= moneyLeft)
                {
                    _onBotUnitChosen?.Invoke(unit);
                    return;
                }
            }
        }
    }
}