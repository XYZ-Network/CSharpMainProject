using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    void Start()
    {
        List<string> names = new List<string> { "Alex", "Boros", "Casha" };
        names.AddRange(names);
        names.Add("Daniel");
        names.Insert(4, "Eagle");
        //visa pisun


        Debug.Log(names[1]);
        Debug.Log(names[2]);
        Debug.Log(names[3]);
    }

    
    
}
