using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class my_first_script : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        int i = 7;
        int f = 3;
        bool isEqual= i == f;
        bool isNotEqual = i!=f;
        bool islessThan = i <f;
        bool isGreaterThen = i>f;
        bool isLessThenOrEqual = i>=f;
        bool isGreaterOrEqual = i<=f;


        Debug.Log(isEqual);
        Debug.Log(isNotEqual);
        Debug.Log(islessThan);
        Debug.Log(isGreaterThen);
        Debug.Log(isLessThenOrEqual);
        Debug.Log(isGreaterOrEqual);
    }

    
}
