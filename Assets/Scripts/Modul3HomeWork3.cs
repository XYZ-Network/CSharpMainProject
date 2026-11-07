using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Modul3HomeWork3 : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        int drink = 3;
       
        switch (drink)
        {
            case 1:
                Debug.Log("Жигулевское светлое");
                OrderCoffee("1");
                break;
            case 2:
                Debug.Log("Ирландский Эль");
                OrderCoffee("2");
                break;
            case 3:
                Debug.Log("Темное Томское");
                OrderCoffee("3");
                break;
            case 4:
                Debug.Log("Вода");
                OrderCoffee("4");
                break;
            case 5:
                Debug.Log("Абсент");
                OrderCoffee("5");
                break;
            case 6:
                Debug.Log("Водка");
                OrderCoffee("6");
                break;
            case 7:
                Debug.Log("Виски");
                OrderCoffee("7");
                break;
            default:
                Debug.Log("Кофе не выбран");
                break;
        }
    }
    string OrderCoffee(string NumberDrink)
    {
        Debug.Log(NumberDrink);
        return "заказ номер " + NumberDrink;
    }
    
}
