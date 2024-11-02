using System;
using System.Collections.Generic;
using System.Linq;
using Model.Runtime;
using Model.Runtime.Projectiles;
using Model.Runtime.ReadOnly;
using UnityEngine;
using Model.Config;
using UnitBrains.Player;
using Model;
using Utilities;

public class GameInitializer : MonoBehaviour
{
    void Awake()
    {
        Debug.Log("GameInitializer Awake called.");

        // Создание экземпляра RuntimeModel
        RuntimeModel runtimeModelInstance = new RuntimeModel();

        // Добавление логирования перед регистрацией
        Debug.Log("Registering RuntimeModel in ServiceLocator...");

        // Регистрация в ServiceLocator
        ServiceLocator.Register<RuntimeModel>(runtimeModelInstance);
        Debug.Log("RuntimeModel registered.");
    }

    void Start()
    {
        Debug.Log("GameInitializer Start called.");

        // Добавление логирования перед вызовом
        Debug.Log("Attempting to retrieve RuntimeModel from ServiceLocator...");
        ExecuteSomeMethod();
    }

    void ExecuteSomeMethod()
    {
        try
        {
            var runtimeModel = ServiceLocator.Get<RuntimeModel>();
            if (runtimeModel != null)
            {
                Debug.Log("RuntimeModel retrieved successfully.");
            }
            else
            {
                Debug.LogError("Retrieved RuntimeModel is null.");
            }
        }
        catch (KeyNotFoundException ex)
        {
            Debug.LogError($"Failed to get RuntimeModel: {ex.Message}");
        }
        catch (InvalidCastException ex)
        {
            Debug.LogError($"Failed to cast service: {ex.Message}");
        }
    }
}