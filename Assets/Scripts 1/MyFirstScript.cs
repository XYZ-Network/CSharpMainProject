using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class MyFirstScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        List<int> ints = new List<int> { 1, 2, 3, 4 };
        List<int> int2 = ints. ToList();
        int2.Add(6);


        foreach (int i in ints)
        {
            Debug.Log(i);
        }

    }   
    

}








    



































