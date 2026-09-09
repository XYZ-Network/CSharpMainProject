using PlasticPipe.PlasticProtocol.Messages;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;

public class MyFirstScript : MonoBehaviour
{

    void Start()
    {
        float[] floats = { -3, -1, 0, 1.3f, 6, 3, 10, 4, 2, 5, -2 };
        float min = float.MaxValue;
        float max = float.MinValue; 
        //for (int i = 0; i < floats.Length; i++)
        //{
        //    if (floats[i] < min) min = floats[i];
        //    if (floats[i] > max) max = floats[i];
        //}
        foreach( float f in floats ) 
        {
            if (f > max) max = f;
            if (f < min) min = f;
        }

        Debug.Log(min);
        Debug.Log(max);
    }
}