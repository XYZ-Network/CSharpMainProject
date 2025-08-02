using System.Collections.Generic;
using Model;
using System.Linq;
using Model.BuffSystem;
using Model.Runtime.Projectiles;
using Model.Runtime.ReadOnly;
using UnityEngine;
using Utilities;
using View;
using Unit = Model.Runtime.Unit;

namespace UnitBrains.Player
{
    public class BufferUnitBrain : BaseUnitBrain
    {
        public override string TargetUnitName => "Buffer"; // Указываем имя юнита
        public override bool IsPlayerUnitBrain => true; // Это юнит игрока

        private const float BuffCooldown = 5f; // Период между баффами
        private const float BuffDuration = 3f; // Длительность баффа
        private const float StopDuration = 0.5f; // Длительность остановки

        private float _lastBuffTime;
        private float _stopEndTime;
        private bool _isStopped;

        public override void Update(float deltaTime, float time)
        {
            if (unit.IsDead) return;

            // Проверяем, нужно ли выйти из состояния остановки
            if (_isStopped && time >= _stopEndTime)
            {
                _isStopped = false;
            }

            // Если в режиме остановки - не действуем
            if (_isStopped) return;

            // Проверяем возможность наложения баффа
            if (time >= _lastBuffTime + BuffCooldown)
            {
                TryApplyBuff(time);
            }
        }

        protected override List<Vector2Int> SelectTargets()
        {
            // Этот юнит не атакует
            return new List<Vector2Int>();
        }

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            // Не генерируем снаряды
        }

        private void TryApplyBuff(float currentTime)
        {
            var allies = GetAlliesInRange();
            if (allies.Count == 0) return;

            // Находим союзника без баффов
            var target = allies.FirstOrDefault(a => !HasBuff(a));
            if (target == null) return;

            // Останавливаемся перед баффом
            _isStopped = true;
            _stopEndTime = currentTime + StopDuration * 2; // Остановка до и после

            // Накладываем бафф после первой половины остановки
            if (currentTime >= _lastBuffTime + BuffCooldown + StopDuration)
            {
                ApplyBuffTo(target);
                _lastBuffTime = currentTime;

                // Визуальный эффект
                var vfx = ServiceLocator.Get<VFXView>();
                vfx?.PlayVFX(unit.Pos, VFXView.VFXType.BuffApplied);
            }
        }

        private List<IReadOnlyUnit> GetAlliesInRange()
        {
            return GetUnitsInRadius(unit.Config.AttackRange, false)
                .Where(u => u != unit && u.Config.IsPlayerUnit)
                .ToList();
        }

        private bool HasBuff(IReadOnlyUnit unit)
        {
            var buffSystem = ServiceLocator.Get<BuffSystemManager>();
            return buffSystem?.GetModifier(unit, BuffType.MoveSpeed) != 1f ||
                   buffSystem?.GetModifier(unit, BuffType.AttackSpeed) != 1f;
        }

        private void ApplyBuffTo(IReadOnlyUnit target)
        {
            var buffSystem = ServiceLocator.Get<BuffSystemManager>();
            if (buffSystem == null) return;

            // Бафф скорости атаки (+30%)
            buffSystem.ApplyBuff(target, new BuffDebuff(BuffType.AttackSpeed, 1.3f, BuffDuration));

            // Бафф скорости движения (+20%)
            buffSystem.ApplyBuff(target, new BuffDebuff(BuffType.MoveSpeed, 1.2f, BuffDuration));
        }

        public override Vector2Int GetNextStep()
        {
            if (_isStopped)
                return unit.Pos;

            // Обычное движение к базе противника
            var enemyBasePos = runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId];
            var path = new AStarUnitPath(runtimeModel, unit.Pos, enemyBasePos);
            return path.GetNextStepFrom(unit.Pos);
        }
    }
}