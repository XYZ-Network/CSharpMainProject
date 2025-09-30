using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCoroutines : MonoBehaviour
{

    private float _counter;
    // Start is called before the first frame update
    void Start()
    {
        Coroutine1(10f);
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(_counter);
    }

    private void Coroutine1(float duration)
    {
        StartCoroutine(Countdown(duration));
    }

    private IEnumerator Countdown(float duration)
    {
        _counter = duration;
        while (_counter > 0)
        {
            _counter--;
            Debug.Log("Короутина работает");
            yield return new WaitForSeconds(1);  //Если всё норм, попробовать 0.1 сделать
        }
    }
}
