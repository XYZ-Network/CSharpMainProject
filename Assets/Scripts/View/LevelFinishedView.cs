using System;
using UnityEngine;

namespace View
{
    public class LevelFinishedView : MonoBehaviour
    {
        [SerializeField] private RectTransform _wonRoot;
        [SerializeField] private RectTransform _loseRoot;
        
        private Action _onClick;

        public void Setup(bool playerWon, Action onClick)
        {
            _wonRoot.gameObject.SetActive(playerWon);
            _loseRoot.gameObject.SetActive(!playerWon);

            _onClick = onClick;
        }
        
        public void OnClick()
        {
            _onClick?.Invoke();
        }
    }
}