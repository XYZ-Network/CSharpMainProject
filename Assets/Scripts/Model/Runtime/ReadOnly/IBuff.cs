using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Model.Runtime
{

    public interface IBuff 
    {
        float Duration { get; }

        float TimeLeft { get; set; }

        void Apply(Unit target);

        void Remove(Unit target);
    }

}

