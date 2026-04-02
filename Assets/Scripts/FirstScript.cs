using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstScript : MonoBehaviour
{
    void Start()
    {
        Summa(8, 11);
        Debug.Log("zo");
    }
    void Summa(int a, int b)
    {
        Debug.Log(a + b);
    }
}

    