using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Runtime.ReadOnly;
using Model;
using UnitBrains;
using UnityEngine;
using Utilities;
using static UnityEngine.GraphicsBuffer;

namespace Assets.Scripts.UnitBrains.Player
{
    public class UnitCordinator
    {
        static UnitCordinator _instance;

        private IReadOnlyRuntimeModel _runtimeModel;
        private TimeUtil _timeUtil;
        public IReadOnlyUnit _recomendTarget = null;

        public UnitCordinator()
        {
            _runtimeModel = ServiceLocator.Get<IReadOnlyRuntimeModel>();
            _timeUtil = ServiceLocator.Get<TimeUtil>();

            _timeUtil.AddUpdateAction(updateTarget);
        }

        public static UnitCordinator GetInstance()
        {
            if (_instance == null)
                _instance = new UnitCordinator();
            return _instance;
        }

        private void updateTarget(float deltaTime)
        {            
            _recomendTarget = null;

            int firstHalfMapX = _runtimeModel.RoMap.Width / 2;
            float closedList = float.MaxValue;
            List<IReadOnlyUnit> Targets = _runtimeModel.RoBotUnits.ToList();

            foreach (IReadOnlyUnit target in Targets)
            {
                if (target.Pos.x < firstHalfMapX && closedList > (target.Pos - _runtimeModel.RoMap.Bases[RuntimeModel.PlayerId]).magnitude)
                {
                    closedList = (target.Pos - _runtimeModel.RoMap.Bases[RuntimeModel.PlayerId]).magnitude;
                    _recomendTarget = target;
                }

            }

            if (_recomendTarget != null)
            {
                return;
            }


            int minHP = int.MaxValue;
            foreach (IReadOnlyUnit target in Targets)
            {
                if (minHP > target.Health)
                {
                    minHP = target.Health;
                    _recomendTarget = target;
                }
            }





        }

    }
}
