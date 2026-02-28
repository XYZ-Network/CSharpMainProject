using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Model.Runtime
{
    // Интерфейс для хранения баффов в коллекциях (нединамический)
    public interface IBuff
    {
        float Duration { get; set; }
        bool CanApply(Unit unit);
        void ApplyNonGeneric(Unit unit);
        void RemoveNonGeneric(Unit unit);
        string GetDescription();
    }

    public abstract class Buff<T> : IBuff where T : Unit
    {
        public float Duration { get; set; }
        public float Modifier { get; set; }

        protected Buff(float duration, float modifier)
        {
            Duration = duration;
            Modifier = modifier;
        }

        // Можно ли применить этот бафф к конкретному юниту?
        public virtual bool CanApply(Unit unit) => unit is T;

        // Нединамические методы для вызова через интерфейс
        public void ApplyNonGeneric(Unit unit)
        {
            if (unit is T typedUnit)
                Apply(typedUnit);
        }

        public void RemoveNonGeneric(Unit unit)
        {
            if (unit is T typedUnit)
                Remove(typedUnit);
        }

        // Применить эффект (вызывает публичный метод юнита)
        public abstract void Apply(T unit);

        // Снять эффект (откатить изменения)
        public abstract void Remove(T unit);
        public abstract string GetDescription();
    }
}
