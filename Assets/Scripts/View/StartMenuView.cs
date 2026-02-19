using System;
using UnityEngine;

namespace View
{
    public class StartMenuView : MonoBehaviour
    {
        private Action _onStartClick;
        private Action _onResetProgressClick;
        
        public void Setup(Action onStartClick, Action onResetProgressClick)
        {
            _onStartClick = onStartClick;
            _onResetProgressClick = onResetProgressClick;    
        }
        
        public void OnStartClick()
        {
            _onStartClick?.Invoke();
        }
        
        public void OnResetProgressClick()
        {
            _onResetProgressClick?.Invoke();
        }
    }
}