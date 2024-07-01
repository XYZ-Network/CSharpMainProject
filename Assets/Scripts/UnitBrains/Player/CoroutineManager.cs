using System.Collections;
using UnityEngine;

public class CoroutineManager : MonoBehaviour
{
    private static CoroutineManager _instance;

    public static CoroutineManager Instance
    {
        get
        {
            if (_instance == null)
            {
                var obj = new GameObject("CoroutineManager");
                _instance = obj.AddComponent<CoroutineManager>();
                GameObject.DontDestroyOnLoad(obj);
            }
            return _instance;
        }
    }
}

