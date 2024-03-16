using UnityEngine;

namespace Model.Runtime.Projectiles
{
    public class ArchToTileProjectile : BaseProjectile
    {
        private const float ProjectileSpeed = 7f;
        private readonly Vector2Int _target;
        private readonly float _timeToTarget;
        private readonly float _totalDistance;
        
        public ArchToTileProjectile(Unit unit, Vector2Int target, int damage, Vector2Int startPoint) : base(damage, startPoint)
        {
            _target = target;
            _totalDistance = Vector2.Distance(StartPoint, _target);
            _timeToTarget = _totalDistance / ProjectileSpeed;
        }

        protected override void UpdateImpl(float deltaTime, float time)
        {
            float timeSinceStart = time - StartTime;
            float t = timeSinceStart / _timeToTarget;
            
            Pos = Vector2.Lerp(StartPoint, _target, t);
            
            float localHeight = 0f;
            float totalDistance = _totalDistance;

            var maxHeight = totalDistance * 0.6f;

            // Не особо понимаю как это нормально назвать,
            // Вероятно это модификатор высоты от времени
            var heightByTimeModifier = t * 2 - 1;

            /* Вероятно в рабочем проете это было бы частью формулы,
             * а не отдельной переменной, но в рамках обучения,
             * ввиду инструкций - вынес в отдельную
            /*/
            var heightByTimeModifierPower2 = Mathf.Pow(heightByTimeModifier, 2);

            /*
             * Отдельно хочу отметить, что не вижу особого смысла 
             * в слеше перед закрывающим комментарием, как было показано в 1м уроке
             * 
             * Так же не вижу смысла в изначальном присвоении localHeight = 0f
             * на это ругается даже компилятор
             * 
             * А так же вижу смысл вынесения 0.6f в приватное поле,
             * однако по заданию код мы можем писать только тут
             * 
             * Так же имя t - не говорящее, без перехода к инициализации не определить что в ней находится
             * похоже, что это процент прошедшего времени или что-то подобное.
             */

            localHeight = maxHeight * (-heightByTimeModifierPower2 + 1);
            
            Height = localHeight;
            if (time > StartTime + _timeToTarget)
                Hit(_target);
        }
    }
}