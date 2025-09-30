using Assets.Scripts.Model.Runtime;
using Model;
using Model.Config;
using UnityEngine;
using Utilities;
using View;

namespace Controller
{
    public class RootController
    {
        private readonly PersistedModel _persisted;
        private readonly RuntimeModel _runtimeModel;
        private readonly LevelController _levelController;
        
        private RootView _rootView;

        public RootController(Settings settings, Canvas targetCanvas)
        {
            _persisted = PersistanceUtils.LoadSingleton(new PersistedModel());

            var timeUtil = TimeUtil.Create();
            ServiceLocator.Register(timeUtil);
            
            _runtimeModel = new RuntimeModel();
            ServiceLocator.Register(_runtimeModel);
            ServiceLocator.RegisterAs(_runtimeModel, typeof(IReadOnlyRuntimeModel));

            var baseAttackDetector = new BaseAttackDetector(_runtimeModel);
            ServiceLocator.RegisterAs(baseAttackDetector, typeof(IBaseAttackDetector));

            var buffsController = new BuffsController();
            ServiceLocator.Register(buffsController);

            SpawnRootVisual(targetCanvas);
            ServiceLocator.Register(_rootView);
            
            var gameplayVisual = SpawnGameplayVisual();
            ServiceLocator.Register(gameplayVisual);

            var vfxView = SpawnVFXView();
            ServiceLocator.Register(vfxView);
            
            _levelController = new(_runtimeModel, this);
            
            _rootView.ShowStartMenu();
        }

        public void RestartGame()
        {
            _runtimeModel.Level = _persisted.Level;
            _levelController.StartLevel(_persisted.Level);
        }

        public void OnLevelFinished(bool playerWon)
        {
            if (playerWon)
            {
                _persisted.IncLevel();
            }

            RestartGame();
        }

        public void ResetProgress()
        {
            _persisted.ResetLevel();
        }

        private void SpawnRootVisual(Canvas targetCanvas)
        {
            var prefab = Resources.Load<RootView>("View/RootView");
            _rootView = Object.Instantiate(prefab, targetCanvas.transform);
            _rootView.Initialize(this);
        }
        
        private Gameplay3dView SpawnGameplayVisual()
        {
            var prefab = Resources.Load<Gameplay3dView>("View/Gameplay3dView");
            return Object.Instantiate(prefab, Vector3.zero, Quaternion.identity);
        }
        
        private VFXView SpawnVFXView()
        {
            var prefab = Resources.Load<VFXView>("View/VFXView");
            return Object.Instantiate(prefab, Vector3.zero, Quaternion.identity);
        }
    }
}