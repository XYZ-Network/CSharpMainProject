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
using System.Threading.Tasks;

public class GameInitializer : MonoBehaviour
{
    void Start()
    {
        Debug.Log("GameInitializer Start called.");
        InitializeGame(); // Инициализируем игру
        ExecuteSomeMethod(); // Вызываем здесь, после регистрации
    }

    private void InitializeGame()
    {
        Debug.Log("GameInitializer InitializeGame called.");

        try
        {
            // Создание и регистрация RuntimeModel
            RuntimeModel runtimeModelInstance = CreateRuntimeModel();

            if (runtimeModelInstance != null)
            {
                ServiceLocator.Register<RuntimeModel>(runtimeModelInstance);
                Debug.Log("RuntimeModel registered.");
            }
            else
            {
                Debug.LogError("Failed to create RuntimeModel.");
            }

            // Проверка успешной регистрации
            if (ServiceLocator.Contains<RuntimeModel>())
            {
                Debug.Log("RuntimeModel successfully registered and available in ServiceLocator.");
            }
            else
            {
                Debug.LogError("RuntimeModel is not registered after creation.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"An error occurred while initializing services: {ex}");
        }
    }

    private RuntimeModel CreateRuntimeModel()
    {
        // Ваша логика создания RuntimeModel
        return new RuntimeModel(); // Убедитесь, что это не null
    }

    private void ExecuteSomeMethod()
    {
        Debug.Log("Attempting to retrieve RuntimeModel...");
        try
        {
            if (ServiceLocator.Contains<RuntimeModel>())
            {
                var runtimeModel = ServiceLocator.Get<RuntimeModel>();
                Debug.Log("RuntimeModel retrieved successfully.");
            }
            else
            {
                Debug.LogError("RuntimeModel is not registered.");
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