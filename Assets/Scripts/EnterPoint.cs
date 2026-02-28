using Controller;
using Model;
using Model.Config;
using Model.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;
//Comment for example FORK
public class EnterPoint : MonoBehaviour
{
    [SerializeField] private Settings _settings;
    [SerializeField] private Canvas _targetCanvas;
    private float _timeScale = 1;
    
    void Start()
    {
        Time.timeScale = _timeScale;
        _settings.LoadPrefabs();
        ServiceLocator.Register(_settings);

        var buffSystem = new BuffSystem();
        ServiceLocator.Register<BuffSystem>(buffSystem);

        var rootController = new RootController(_settings, _targetCanvas);
        ServiceLocator.Register(rootController);
    }
}
