using Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnitBrains.Player;
using UnityEngine;
using Utilities;

namespace UnitBrains.Player
{
    public class ThirdUnitBrain : DefaultPlayerUnitBrain
    {
        // Start is called before the first frame update
        public override string TargetUnitName => "Ironclad Behemoth";
        public enum ModeTypes
        {
            None,
            Attack,
            Move
        }
        public ModeTypes Mode = ModeTypes.Move;
        public ModeTypes EndMode = ModeTypes.None;

        public bool Delay = false;
        public float DelayTime = 1f;
        public float StartTimer = 0f;


        public override Vector2Int GetNextStep()
        {
            if (Mode == ModeTypes.Move)
            {
                Debug.Log("Move!");
                return base.GetNextStep();
            }
            else
            {
                return unit.Pos;
            }
        }

        protected override List<Vector2Int> SelectTargets()
        {
            if (Mode == ModeTypes.Attack)
            {
                Debug.Log("Attack!");
                return base.SelectTargets();
            }
            else
            {
                return new List<Vector2Int>();
            }
        }

        private void DelayWithChangedMode()
        {
            if (Delay == false)
            {
                StartTimer = 0;
            }
            Delay = true;
        }

        private void CheckDelay()
        {
            Debug.Log(StartTimer);
            if (Delay && StartTimer >= DelayTime)
            {
                UnDelayWithChangedMode();
                Debug.Log("ATTTTAAACK!!!!");
                Mode = EndMode;
            }
        }

        private void UnDelayWithChangedMode()
        {
            Delay = false;
        }

        private void ChangeModeToAttack()
        {
            if (Mode != ModeTypes.Attack)
            {
                Mode = ModeTypes.None;
                DelayWithChangedMode();
            }
            EndMode = ModeTypes.Attack;
        }
        private void ChangeModeToMove()
        {
            if (Mode != ModeTypes.Move)
            {
                Mode = ModeTypes.None;
                DelayWithChangedMode();
            }
            EndMode = ModeTypes.Move;
        }
        public override void Update(float deltaTime, float time)
        {
            StartTimer += deltaTime * 10;   /// ¬от здесь что то не так!!!!!!!! StartTimer нужен дл€ отсчета 1 секунды, он также выводитс€ в CheckDelay дл€ проверки.
            CheckDelay();
            if (HasTargetsInRange())
            {
                ChangeModeToAttack();
            }
            else
            {
                ChangeModeToMove();
            }
        }
    }
}
