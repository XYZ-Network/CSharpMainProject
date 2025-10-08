using System;
using System.Collections.Generic;
using System.Reflection;
using UnitBrains;
using UnityEngine;

namespace Model.Runtime
{
    public static class UnitExtensions
    {
        private static readonly FieldInfo BrainField = typeof(Unit).GetField("_brain", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo SelectTargetsMethod = typeof(BaseUnitBrain).GetMethod("SelectTargets", BindingFlags.NonPublic | BindingFlags.Instance);

        public static BaseUnitBrain GetBrain(this Unit unit)
        {
            return BrainField?.GetValue(unit) as BaseUnitBrain;
        }

        public static List<Vector2Int> GetAttackTargets(this Unit unit)
        {
            var brain = unit.GetBrain();
            if (brain == null) return new List<Vector2Int>();

            return SelectTargetsMethod?.Invoke(brain, null) as List<Vector2Int> ?? new List<Vector2Int>();
        }
    }
}