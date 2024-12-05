using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class SingleCoroutines : MonoBehaviour
{
    private static SingleCoroutines instanse
    {
        get
        {
            if (m_instanse == null)
            {
                var go = new GameObject("[Coroutine manager]");
                m_instanse = go.AddComponent<SingleCoroutines>();
                DontDestroyOnLoad(go);
            }
            return m_instanse;
        }
    }

    private static SingleCoroutines m_instanse;
    public static Coroutine StartRoutine(IEnumerator enumerator)
    {
        return instanse.StartCoroutine(enumerator);
    }

    public static void StopRoutine(Coroutine routine)
    {
        if(routine != null) 
            instanse.StopCoroutine(routine);
    }
}
