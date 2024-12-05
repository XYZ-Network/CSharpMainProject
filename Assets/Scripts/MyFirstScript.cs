using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyFirstScript : MonoBehaviour
{
    int A = 2;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(A);
        Prinimachka(ref A);
        Debug.Log(A);


    }
    void Prinimachka(ref int l)
    {
        l++;
    }

    








}
