using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MayFirstProject : MonoBehaviour
{
    void Start()
    {
        //int i = 0;

        //  while (i < 10)
        //  {
        //      i++;
        //      if (i % 2 == 0)
        //      {
        //          continue;
        //      }
        //      Debug.Log(i);
        //  }

        //for (int a = 0; a < 10; a++)
        //{
        //    Debug.Log(a);
        //}
        // Инициализируем переменные для суммы и произведения
        float n = 25;
        int a = 0;
        int b = 1;

        // 2. Используем цикл for для вычисления суммы и произведения
        for (int i = 1; i <= n; i++)
        {
            a += i; // Находим сумму
            b *= i; // Находим произведение
        }

        // 3. Выводим полученную сумму и произведение в консоль
        Debug.Log($"Сумма чисел от 1 до {n} равна {a}");
        Debug.Log($"Произведение чисел от 1 до {n} равно {b}");
    }




}




    // Start is called before the first frame update
    //void Start()
    //{
    //    Debug.Log("Введите номер выбранного напитка: ");
    //    int drink = Convert.ToInt32(Console.ReadLine());

    //    switch (drink)
    //    {
    //        case 1:
    //            OrderCoffee("Cappuccino");
    //            break;
    //        case 2:
    //            OrderCoffee("Латте");
    //            break;
    //        case 3:
    //            OrderCoffee("Американо");
    //            break;
    //        case 4:
    //            OrderCoffee("Какао");
    //            break;
    //        case 5:
    //            OrderCoffee("Горячий шоколад");
    //            break;
    //        case 6:
    //            OrderCoffee("Чай с лимоном");
    //            break;
    //        case 7:
    //            OrderCoffee("Горячая вода");
    //            break;
    //        default:
    //            Debug.Log("Неправильный выбор. Пожалуйста, выберите номер от 1 до 7.");
    //            break;
    //    }
    //}

    //static void OrderCoffee(string drinkName)
    //{
    //    Debug.Log($"Вы выбрали: {drinkName}");
    //}

