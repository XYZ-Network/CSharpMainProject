using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITransientEffect
{
    int Time { get; set; }

    void ReduceTime();
}
