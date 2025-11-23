using System.Collections.Generic;
using System.Linq;
using Model;
using Model.Runtime.Projectiles;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using Utilities;
using static UnityEngine.GraphicsBuffer;


namespace UnitBrains.Player
{
    public class ThirdUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Ironclad Behemoth";

        private enum BehemothState { Moving, Shooting, Switching }
        private BehemothState _state = BehemothState.Moving;
        private BehemothState _nextState = BehemothState.Moving;
        List<Vector2Int> Outreachable = new List<Vector2Int>();

        private const float SwitchDuration = 1f;
        private float _switchTimer = 0f;
        private static int UnitCounter = 0;
        public int Counter { get; private set; }
        private const int MaxTargets = 3;

        public ThirdUnitBrain()
        {
            Counter = UnitCounter++;
        }
        public override void Update(float deltaTime, float time)
        {
            // 1) КАЖДЫЙ КАДР решаем, какой режим нужен СЕЙЧАС
            bool hasTargetsInRange = false;
            foreach (var t in GetAllTargets())
            {
                if (IsTargetInRange(t)) { hasTargetsInRange = true; break; }
            }
            if (_state != BehemothState.Switching) // во время переключения не передумываем
                DecideDesiredState(hasTargetsInRange);

            // 2) Отрабатываем таймер переключения
            if (_state == BehemothState.Switching)
            {
                _switchTimer += deltaTime;
                if (_switchTimer >= SwitchDuration)
                {
                    _state = _nextState;
                    _switchTimer = 0f;
                    // Debug.Log($"[Behemoth] State = {_state}");
                }
            }

            // 3) Только теперь — базовая логика (она уже увидит актуальный _state)
            base.Update(deltaTime, time);
        }

        // В режиме Moving/Switching — целей не отдаём (атака блокируется)
        protected override List<Vector2Int> SelectTargets()
        {
            List<Vector2Int> targets = new List<Vector2Int>();
            List<Vector2Int> result = new List<Vector2Int>();

            if (_state != BehemothState.Shooting)
            
                return result;

                Outreachable.Clear();
                foreach (var target in GetAllTargets())
                {
                    if (IsTargetInRange(target))
                    {
                        targets.Add(target);

                    }
                    else
                    {
                        Outreachable.Add(target);
                    }
                }


                if (targets.Count == 0 && Outreachable.Count == 0)
                {
                    if (IsTargetInRange(runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId]))
                    {
                        result.Add(runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId]);
                        return result;
                    }
                    Outreachable.Add(runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId]);
                    return result;
                }

                SortByDistanceToOwnBase(targets);
                int targetID = Counter % MaxTargets;
                // Индекс таргета зависит от максимального кол-ва целей и айди нашего атакующего юнита 
                if (targetID < targets.Count)
                {
                    result.Add(targets[targetID]);
                }
                else if (targets.Count > 0)
                {
                    result.Add(targets[targets.Count - 1]); // последний элемент, ВАЖНО индекс положительный
                }

            return result;
        }

        // Движемся только в Moving
        public override Vector2Int GetNextStep()
        {
            if (_state != BehemothState.Moving)
                return unit.Pos;

            return base.GetNextStep();
        }

        // Явно создаём пулю (одна пуля на тик задержки — задаётся в префабе)
        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            if (_state != BehemothState.Shooting) return;

            var p = CreateProjectile(forTarget);
            AddProjectileToList(p, intoList);
            Debug.Log("[Behemoth] Fire!");
        }

        private void DecideDesiredState(bool hasTargetsInRange)
        {
            var desired = hasTargetsInRange ? BehemothState.Shooting : BehemothState.Moving;
            if (desired != _state)
                BeginSwitch(desired);
        }

        private void BeginSwitch(BehemothState to)
        {
            if (_state == BehemothState.Switching) return; // уже переключаемся
            _state = BehemothState.Switching;
            _nextState = to;
            _switchTimer = 0f;
            Debug.Log($"[Behemoth] Switching → {_nextState}");
        }
    }
}
