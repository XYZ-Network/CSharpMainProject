using System;
using UnityEngine;
using Utilities;

namespace Model
{
    [Serializable]
    public class PersistedModel
    {
        [SerializeField] private int _level = 0;

        public int Level => _level;

        public void IncLevel()
        {
            _level++;
            PersistanceUtils.PersistSingleton(this);
        }

        public void ResetLevel()
        {
            _level = 0;
            PersistanceUtils.PersistSingleton(this);
        }
    }
}