using Model;
using Model.Runtime;
using Model.Runtime.ReadOnly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Utilities;
using static UnityEditor.PlayerSettings;
using static UnityEngine.UI.CanvasScaler;
using Unit = Model.Runtime.Unit;

namespace Assets.Scripts.Utilities
{
    public class UnitsTargetManager
    {
        private IReadOnlyRuntimeModel _runtimeModel;
        private TimeUtil _timeUtil;
        private int _playerId;
        public IReadOnlyUnit recomendTarget = null;

        public UnitsTargetManager(IReadOnlyRuntimeModel runtimeModel, TimeUtil timeUtil, int playerId)
        {
            _runtimeModel = runtimeModel;
            _timeUtil = timeUtil;
            _playerId = playerId;

            _timeUtil.AddUpdateAction(updateTarget);
        }

        private void updateTarget(float deltaTime)
        {
            
            recomendTarget = null;

            if (_runtimeModel.RoMap == null)
                return;

            //Ищем на нашей половине карты ближайшего врага к нашей базе
            int firstHalfMapX = _runtimeModel.RoMap.Width / 2;
            float closedDist = float.MaxValue;

            IEnumerable<IReadOnlyUnit> units = _playerId == RuntimeModel.PlayerId ? _runtimeModel.RoBotUnits : _runtimeModel.RoPlayerUnits;

            List<IReadOnlyUnit> Targets = units.ToList();

            foreach (IReadOnlyUnit target in Targets)
            {
                if (target.Pos.x < firstHalfMapX && closedDist > (target.Pos - _runtimeModel.RoMap.Bases[_playerId]).magnitude)
                {
                    closedDist = (target.Pos - _runtimeModel.RoMap.Bases[_playerId]).magnitude;
                    recomendTarget = target;
                }
                    
            }
            //если найден - завершаем поиски
            if (recomendTarget != null) { 
                return; 
            }

            //ищем врага с минимальным HP
            int minHP = int.MaxValue;
            foreach (IReadOnlyUnit target in Targets)
            {
                if (minHP > target.Health)
                {
                    minHP = target.Health;
                    recomendTarget = target;
                }
            }

            

            //ps: dorogoi dnevnik, mne ne podobrat' slov chtobi opisat' bol' i unizhenie, kotorie ia ispital segodnia.... 

            
        }

    }
}
