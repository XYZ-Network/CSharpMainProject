using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class MyScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        int timeHour = 18;
        bool isWeekend = true;
        bool isEvening = (timeHour >= 18);
        bool haveTicket = true;

        bool goToCinema = (isWeekend || isEvening) && haveTicket;
        Debug.Log(goToCinema);
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
