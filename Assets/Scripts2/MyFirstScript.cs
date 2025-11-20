using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    

    // Start is called before the first frame update
    void Start()
    {
        int a = 5;
        

        Numbers(a);
        Debug.Log("a var = " + a);
  

    }
    
    void Numbers(int i)
    {
        i *= 3;
        Debug.Log("New var = " + i);
        
    }
    

        

    
        
    


}









