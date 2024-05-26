using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class Petrosyan : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
       List<string> names = new List<string> { "Alex","Nik","John"};
        List<string> femnames = new List<string> { "Soni", "Lency", "Ada" }; 
        names.Add("James");
        names.Insert(1, "Daniel");
        names.AddRange(femnames);
        names.Add("John");
       foreach (string name in names)
        {
            Debug.Log(name);
        }



    }    









}



    
        

       
    



   
  
        

   

        








    
    
       
   

   