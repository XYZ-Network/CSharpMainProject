using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class MyFirstScript : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        int i = 3;
        int o = 5;
        Debug.Log(i * o);
        bool isequal = i == 3;
        Debug.Log(isequal);


    }
}
