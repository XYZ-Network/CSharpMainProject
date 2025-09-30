using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Profiling;
using UnityEngine;

public class MyFirstScriptUpdate : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        float[] floats = { -3, -1, 0, 1.3f, 1, -9, 6, 3, 10, 4, 2, 5, -2 };
        float min = float.MaxValue;

        foreach (float f in floats)
        {
            if (f < min) min = f;
        }
        Debug.Log(min);
    }


}
