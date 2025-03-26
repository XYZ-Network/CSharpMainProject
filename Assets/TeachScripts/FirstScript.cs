using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        float firstNumber = 10;
        float secondNumber = 5;
        float summ;
        float subtraction;

        summ = GetSumm(firstNumber, secondNumber);
        subtraction = GerSubtraction(firstNumber, secondNumber);

        ConsoleLog(summ);
        ConsoleLog(subtraction);
    }

    float GetSumm(float firstNumber, float secondNumber)
    {
        return firstNumber * secondNumber;
    }

    float GerSubtraction(float firstNumber, float secondNumber)
    {
        return firstNumber - secondNumber;
    }

    void ConsoleLog(float result)
    {
        Debug.Log(result);
    }
}
