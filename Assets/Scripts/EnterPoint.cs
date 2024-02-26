using System.Collections;
using System.Collections.Generic;
using Controller;
using Model;
using Model.Config;
using UnityEngine;
using Utilities;

public class EnterPoint : MonoBehaviour
{
    [SerializeField] private Settings _settings;
    [SerializeField] private Canvas _targetCanvas;
    private float _timeScale = 1;

    //Привет
    /*/ Супер большой коментарий
    И тут
    И здесь
    Даже тут
    *в уроке звёздочка зачем-то перед началом комента, хотя коментарий, вроде, и без неё работает
    Спасибо за внимание
    /*/

    //Ещё раз привет
    /*/ Вспоминаю полученные знания
    Пожелайте мне удачи!
    /*/

    void Start()
    {
        Time.timeScale = _timeScale;
        _settings.LoadPrefabs();
        ServiceLocator.Register(_settings);
        
        var rootController = new RootController(_settings, _targetCanvas);
        ServiceLocator.Register(rootController);
    }
}
