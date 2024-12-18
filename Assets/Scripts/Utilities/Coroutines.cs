using UnityEngine;

public class Coroutines : MonoBehaviour
{
    public static Coroutines Instance;

    private void Awake()
    {
        Instance = this;
    }
}

