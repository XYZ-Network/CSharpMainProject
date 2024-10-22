using Model;
using Model.Config;
using UnityEngine;
using Utilities;
using View;

// регистрации сервисов происходят тут в RootController
// системы создаются или передаются из вне
namespace Controller
{
    public class RootController
    {
        private readonly PersistedModel _persisted;
        private readonly RuntimeModel _runtimeModel;
        private readonly LevelController _levelController;
        
        // ДЗ-12
        private readonly BuffService _buffService;
        
        private RootView _rootView;

        public RootController(Settings settings, Canvas targetCanvas)
        {
            _persisted = PersistanceUtils.LoadSingleton(new PersistedModel());
            ServiceLocator.Register(TimeUtil.Create());
            
            _runtimeModel = new();
            ServiceLocator.RegisterAs(_runtimeModel, typeof(IReadOnlyRuntimeModel));
            
            SpawnRootVisual(targetCanvas);
            ServiceLocator.Register(_rootView);
            
            var gameplayVisual = SpawnGameplayVisual();
            ServiceLocator.Register(gameplayVisual);

            var vfxView = SpawnVFXView();
            ServiceLocator.Register(vfxView);
            
            _levelController = new(_runtimeModel, this);
            
            _rootView.ShowStartMenu();

            // запись системы // ДЗ-12
            UnitConfig unitConfig = new UnitConfig();
            TimeUtil timeUtil = ServiceLocator.Get<TimeUtil>();
            _buffService = new BuffService(new CharacterStats(unitConfig.MoveDelay, unitConfig.AttackDelay), timeUtil);
            ServiceLocator.Register(_buffService);
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