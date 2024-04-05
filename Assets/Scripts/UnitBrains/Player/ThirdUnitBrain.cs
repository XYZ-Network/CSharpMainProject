using System.Collections.Generic;
using UnitBrains.Player;
using UnityEngine;

public class ThirdUnitBrain : DefaultPlayerUnitBrain
{
    public override string TargetUnitName => "Ironclad Behemoth";

    private float transitionTimer = 1f; 
    private float currentTransitionTime = 0f; 

    private bool isAttacking = false; 
    private bool isMoving = false; 
    public override Vector2Int GetNextStep()
    {
        isMoving = true;


        return base.GetNextStep(); 



      
        
    }

   
    protected override List<Vector2Int> SelectTargets()
    {
        isAttacking = true;

        Debug.Log(isAttacking);

        
        return base.SelectTargets(); 
       
      
    }

 
    public override void Update(float deltaTime, float time)
    {
        base.Update(deltaTime, time);
    }
}








