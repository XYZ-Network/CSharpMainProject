using BuffSystem;
using Controller;
using Model;
using Model.Config;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

public class EnterPoint : MonoBehaviour
{
    [SerializeField] private Settings _settings;
    [SerializeField] private Canvas _targetCanvas;
    private float _timeScale = 1; //Тут мы увеличели скорость игры
    
    void Start()
    {
        Time.timeScale = _timeScale;
        _settings.LoadPrefabs();
        ServiceLocator.Register(_settings);

        var buffSystem = new BuffManager();
        buffSystem.Initialize(this); // 'this' — EnterPoint как MonoBehaviour
        ServiceLocator.Register<IBuffSystem>(buffSystem);

        var rootController = new RootController(_settings, _targetCanvas);
        ServiceLocator.Register(rootController);
    }
}
