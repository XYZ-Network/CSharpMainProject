using System.Collections.Generic;
using System.Linq;
using Model.Runtime.Buffs;
using Model.Runtime.ReadOnly;
using UnityEngine;
using Utilities;
using View;

namespace UnitBrains.Player
{
   
    public sealed class FourthUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Buffer";

        // Раз в 3 секунды Buffer может попытаться выдать новый бафф.
        private const float BuffInterval = 3f;

        // Остановка перед применением баффа.
        private const float PauseBeforeBuff = 0.5f;

        // Остановка после применения баффа.
        private const float PauseAfterBuff = 0.5f;

        // Время жизни баффа.
        private const float BuffDuration = 6f;

        // Увеличение скорости движения в 1.5 раза.
        private const float MoveSpeedMultiplier = 1.5f;

        // Увеличение скорости атаки в 1.5 раза.
        private const float AttackSpeedMultiplier = 5f;

        // Время, когда можно будет искать следующую цель.
        private float _nextBuffTime;

        // Время окончания текущей остановки.
        private float _pauseUntil;

        // Время, когда нужно применить подготовленный бафф.
        private float _buffAtTime = -1f;

        // Цель, которой собираемся выдать бафф.
        private IReadOnlyUnit _pendingTarget;

        public override void Update(float deltaTime, float time)
        {
            // Если у нас уже есть запланированный бафф...
            if (_buffAtTime >= 0f)
            {
                // ...но время его применения ещё не наступило,
                // продолжаем стоять.
                if (time < _buffAtTime)
                    return;

                // Время пришло — применяем бафф.
                ApplyBuff(_pendingTarget);

                // Больше цель не нужна.
                _pendingTarget = null;

                // Сбрасываем состояние запланированного баффа.
                _buffAtTime = -1f;

                // После применения ещё 0.5 секунды стоим.
                _pauseUntil = time + PauseAfterBuff;

                // Следующая попытка баффа будет через 3 секунды.
                _nextBuffTime = time + BuffInterval;

                return;
            }

            // Если сейчас ещё идёт пауза или ещё не пришло время следующего баффа — ничего не делаем.
            if (time < _pauseUntil || time < _nextBuffTime)
                return;

            // Ищем ближайшего союзника без баффа.
            var target = FindUnbuffedAlly();

            // Никого не нашли.
            if (target == null)
                return;

            // Запоминаем найденную цель.
            _pendingTarget = target;

            // Останавливаемся на 0.5 секунды перед баффом.
            _pauseUntil = time + PauseBeforeBuff;

            // Через 0.5 секунды применим бафф.
            _buffAtTime = time + PauseBeforeBuff;
        }

        public override Vector2Int GetNextStep()
        {
            // Пока готовимся к баффу или стоим после него, остаёмся на текущей клетке.
            if (Time.time < _pauseUntil || _buffAtTime >= 0f)
                return unit.Pos;

            // В остальное время используем обычное движение.
            return base.GetNextStep();
        }

        protected override List<Vector2Int> SelectTargets()
        {
            // Buffer не умеет атаковать. Возвращаем пустой список целей.
            return new List<Vector2Int>();
        }

        private IReadOnlyUnit FindUnbuffedAlly()
        {
            // Получаем систему баффов.
            var buffSystem = ServiceLocator.Get<IBuffSystem>();

           return GetUnitsInRadius(unit.Config.AttackRange, true)

                // Дополнительно убеждаемся, что это наш союзник.
                .Where(ally =>
                    ally.Config.IsPlayerUnit ==
                    unit.Config.IsPlayerUnit)

                // Берём только юнитов, у которых ещё нет баффа.
                .Where(ally => !buffSystem.HasBuff(ally))

                // Из нескольких целей выбираем ближайшую.
                .OrderBy(ally =>
                    (ally.Pos - unit.Pos).sqrMagnitude)

                // Если никого нет — вернётся null.
                .FirstOrDefault();
        }

        private void ApplyBuff(IReadOnlyUnit target)
        {
            // Если цель исчезла — ничего не делаем.
            if (target == null)
                return;

            // Получаем реальный Unit.
            if (target is not Model.Runtime.Unit concreteTarget)
                return;

            // Мёртвого юнита баффать нельзя.
            if (concreteTarget.IsDead)
                return;

            // Получаем систему баффов.
            var buffSystem = ServiceLocator.Get<IBuffSystem>();

            // За 0.5 секунды ожидания цель могла уже получить бафф от другого Buffer.
            if (buffSystem.HasBuff(concreteTarget))
                return;

            // Создаём бафф.
            var buff = new UnitBuff(
                BuffDuration,
                MoveSpeedMultiplier,
                AttackSpeedMultiplier,
                "BufferSpeedBoost");

            // Добавляем бафф юниту.
            concreteTarget.AddBuff(buff);

            // Диагностическое сообщение в Console.
            Debug.Log(
                $"Buffer applied buff to {concreteTarget.Config.Name}: " +
                $"Move x{buffSystem.GetMoveSpeedMultiplier(concreteTarget)}, " +
                $"Attack x{buffSystem.GetAttackSpeedMultiplier(concreteTarget)}");

            // Получаем VFX-систему.
            var vfxView = ServiceLocator.Get<VFXView>();

            // Показываем эффект именно на союзнике.
            vfxView.PlayVFX(
                concreteTarget.Pos,
                VFXView.VFXType.BuffApplied);
        }
    }
}
