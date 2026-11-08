using System;
using System.Collections.Generic;

namespace Model.Runtime
{
    /// <summary>
    /// Набор модификаторов, привязанных к источнику (источник — сам бафф).
    /// Позволяет складывать несколько баффов и точно снимать их.
    /// </summary>
    public class StatModifiers
    {
        private readonly Dictionary<object, float> _values = new();
        private readonly float _identity;
        private readonly Func<float, float, float> _combine;

        public StatModifiers(float identity, Func<float, float, float> combine)
        {
            _identity = identity;
            _combine = combine;
        }

        public void Set(object source, float value) => _values[source] = value;
        public void Remove(object source) => _values.Remove(source);

        public float Total
        {
            get
            {
                float result = _identity;
                foreach (var v in _values.Values)
                    result = _combine(result, v);
                return result;
            }
        }
    }
}