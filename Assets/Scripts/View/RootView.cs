using Controller;
using UnityEngine;

namespace View
{
    public class RootView : MonoBehaviour
    {
        [SerializeField] private StartMenuView _startMenuView;
        [SerializeField] private LevelFinishedView _levelFinishedView;
        
        private RootController _rootController;
        
        public void Initialize(RootController rootController)
        {
            _rootController = rootController;
            _levelFinishedView.gameObject.SetActive(false);
            _startMenuView.gameObject.SetActive(false);
        }

        public void ShowLevelFinished(bool playerWon)
        {
            _levelFinishedView.Setup(playerWon, () =>
            {
                _levelFinishedView.gameObject.SetActive(false);
                _rootController.RestartGame();
            });
            
            _levelFinishedView.gameObject.SetActive(true);
        }
        
        public void HideLevelFinished() => _levelFinishedView.gameObject.SetActive(false);

        public void ShowStartMenu()
        {
            _startMenuView.Setup(() =>
                {
                    _startMenuView.gameObject.SetActive(false);
                    _rootController.RestartGame();
                },
                _rootController.ResetProgress
            );
            
            _startMenuView.gameObject.SetActive(true);
        }
        
        public void HideStartMenu() => _startMenuView.gameObject.SetActive(false);
    }
}