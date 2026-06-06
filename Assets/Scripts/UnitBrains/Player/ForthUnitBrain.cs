using Baffs;
using Model;
using Model.Runtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using TMPro;
using UnitBrains.Player;
using UnityEngine;
using Utilities;
using View;

namespace UnitBrains.Player
{
    public class ForthUnitBrain : DefaultPlayerUnitBrain
    {
        // Start is called before the first frame update
        public override string TargetUnitName => "Buffer";
        private VFXView _vfxView = new VFXView();


        private enum ModeTypes
        {
            Move,
            BeforeBuff,
            AfterBuff

        }
        private ModeTypes _mode = ModeTypes.Move;

        public bool Delay = false;
        public float DelayTime = 0.5f;
        public float Timer = 0f;


        public override Vector2Int GetNextStep()
        {
            if (_mode == ModeTypes.Move)
            {
                return base.GetNextStep();
            }
            else
            {
                return unit.Pos;
            }
        }

        protected override List<Vector2Int> SelectTargets()
        {
            return new List<Vector2Int>();
        }

        private void GiveBuffTarget()
        {
            List<Vector2Int> targets = GetReachableAllies();
            targets.Sort((y, x) => GetUnitAt(x).Health.CompareTo(GetUnitAt(y).Health));

            BuffAttackRange buffAttackRange = new BuffAttackRange();
            BuffSecondAttack buffSecondAttack = new BuffSecondAttack();

            foreach (Vector2Int target in targets)
            {
                Unit currentUnit = (Unit)GetUnitAt(target);
                if (currentUnit.GetBrain() is SecondUnitBrain && BuffsData.TryAddStatusToData(currentUnit.GetBrain(), buffSecondAttack))
                {
                    _vfxView.PlayVFX(unit.Pos, VFXView.VFXType.BuffApplied);
                    return;
                }
                if (currentUnit.GetBrain() is ThirdUnitBrain && BuffsData.TryAddStatusToData(currentUnit.GetBrain(), buffAttackRange))
                {
                    _vfxView.PlayVFX(unit.Pos, VFXView.VFXType.BuffApplied);
                    return;
                }
            }
            Debug.Log($"{unit.Pos} םו למזוע םאיעט צוכü הכÿ באפפא");
        }

        public override void Update(float deltaTime, float time)
        {
            Timer += deltaTime * 10;

            if (HasAlliesInRange() && _mode == ModeTypes.Move)
            {
                _mode = ModeTypes.BeforeBuff;
                Timer = 0;
            }
            else if (HasAlliesInRange() && _mode == ModeTypes.BeforeBuff && Timer >= DelayTime)
            {
                Timer = 0;
                GiveBuffTarget();
                _mode = ModeTypes.AfterBuff;
            }
            else if (!HasAlliesInRange() && _mode == ModeTypes.BeforeBuff)
            {
                _mode = ModeTypes.Move;
            }
            else if (_mode == ModeTypes.AfterBuff && Timer >= DelayTime){
                _mode = ModeTypes.Move;
            }
        }
    }
}
