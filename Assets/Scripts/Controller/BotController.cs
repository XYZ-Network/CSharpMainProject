using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Model;
using Model.Config;
using UnityEngine;
using Utilities;
//давай по новой
//как сам?
//я неплохо

namespace Controller
{
    public class BotController
    {
        private readonly TimeUtil _timeUtil;
        private readonly IReadOnlyRuntimeModel _runtimeModel;
        private readonly List<UnitConfig> _sortedUnits;
        private readonly Action<UnitConfig> _onBotUnitChosen;
        private Coroutine _updateCoroutine;
        
        public BotController(Action<UnitConfig> onBotUnitChosen)
        {
            _onBotUnitChosen = onBotUnitChosen;
            
            _timeUtil = ServiceLocator.Get<TimeUtil>();
            _runtimeModel = ServiceLocator.Get<IReadOnlyRuntimeModel>();
            _sortedUnits = ServiceLocator.Get<Settings>().EnemyUnits.Keys
                .OrderBy(x => x.Cost).ToList();

            _updateCoroutine = _timeUtil.StartCoroutine(UpdateCoroutine());
        }

        public void Stop()
        {
            _timeUtil.StopCoroutine(_updateCoroutine);
            _updateCoroutine = null;
        }

        private IEnumerator UpdateCoroutine()
        {
            var delay = new WaitForSeconds(1f);
            while (true)
            {
                var stage = _runtimeModel.Stage;
                switch (stage)
                {
                    case RuntimeModel.GameStage.ChooseUnit:
                        ChooseUnit();
                        break;
                }

                yield return delay;
            }
        }

        private void ChooseUnit()
        {
            var moneyLeft = _runtimeModel.RoMoney[RuntimeModel.BotPlayerId];
            if (_sortedUnits[0].Cost > moneyLeft)
                return;

            for (int i = _sortedUnits.Count - 1; i >= 0; i--)
            {
                moneyLeft = _runtimeModel.RoMoney[RuntimeModel.BotPlayerId];
                var unitBudget = moneyLeft / (i + 1);
                var unit = _sortedUnits[i];
                while (unit.Cost <= unitBudget && unit.Cost < moneyLeft)
                {
                    _onBotUnitChosen?.Invoke(unit);
                    unitBudget -= unit.Cost;
                }
            }
        }
    }
}
