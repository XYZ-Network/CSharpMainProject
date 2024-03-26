using System;
using System.Collections;
using System.Collections.Generic;
using UnitBrains.Player;
using UnityEngine;

public class ThirdUnitBrain : DefaultPlayerUnitBrain
   
{
    public override string TargetUnitName => "Ironclad Behemoth";

    private const float TransitionDuration = 1f;
    private float transitionStartTime = 0f;
    private bool Ataka1=false;
    private bool Ataka2=false;


    public void IroncladBehemoth()
    {
        Console.WriteLine("Ironclad Behemoth");
        return; 
    }
    public void spornoe_nazvanie()
    {
        if (Ataka1)
        {
            if (Ataka1 != Ataka2)
            {
                while (TransitionDuration> transitionStartTime)
                {
                    transitionStartTime= transitionStartTime+0.01f;
                }
            }
            Console.WriteLine("¿“¿ ”ﬁ");

            Ataka2 = Ataka1;
           
        }
        else
        {
            if (Ataka1 != Ataka2)
            {
                while (TransitionDuration > transitionStartTime)
                {
                    transitionStartTime = transitionStartTime + 0.01f;
                }
            }
            Console.WriteLine("Â‰Û");

            Ataka2 = Ataka1;
        }


    }


   

  
}
