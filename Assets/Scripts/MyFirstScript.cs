using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class MyFirstScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        int x = 99;
        int sum = 0;
        BigInteger prod = 1;
        for (int i = 0; i < x; i++)
        {
            sum += (i + 1);
            prod *= (i + 1);
            Debug.Log(prod);
        }
        Debug.Log(sum);
        Debug.Log(prod);
        //Debug.Log((x * (x + 1) / 2));
    }
}
