using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyfirstScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        int joy = 0;
        bool goout = (joy >= 0);
        bool exam = true;
        bool gamedevclasses = true;

        bool home = (!goout && !exam) || gamedevclasses;
        Debug.Log(home);

        int hoy = 4;
        hoy += 4;
        hoy -= 20;
        hoy *= 3;
        hoy /= -4;
        hoy++;
        hoy--;
        Debug.Log(hoy);
    }

 
}
