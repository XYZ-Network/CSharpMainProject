using System;
using UnityEngine;

namespace ALTUSHKA.TheAtlas.Samples
{
    // NOTE: These scripts are for demonstration purposes of "The Atlas" plugin only.
    // They do not represent best coding practices.

    public class AtlasDemo_Manager : MonoBehaviour
    {
        // Singleton pattern tracking
        public static AtlasDemo_Manager Instance;

        // Static property tracking
        public static bool IsGamePaused = false;

        // Event subscription tracking
        public event Action OnLevelStart;


        private void Awake()
        {
            Instance = this;
            StartGame();
        }

        public void StartGame()
        {
            OnLevelStart?.Invoke();
            Debug.Log($"Player HP is: {FindFirstObjectByType<AtlasDemo_Player>().Health}");
        }
    }
}