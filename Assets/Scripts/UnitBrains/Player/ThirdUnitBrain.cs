using Model;
using System;
using System.Collections;
using System.Collections.Generic;
using UnitBrains.Pathfinding;
using UnitBrains.Player;
using UnityEngine;


public class ThirdUnitBrain : DefaultPlayerUnitBrain
{
    private const float awake = 1f;
    private float sleep = 0f;
    private enum State
    {
        Move,
        Stay,
        Shoot
    }

    private State _state = State.Move;
    private bool statement = true;
    public override string TargetUnitName => "Ironclad Behemoth";
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public override Vector2Int GetNextStep()
    {
        if(HasTargetsInRange())
        {
            statement = false;
            _state = State.Stay;
            return unit.Pos;
        }
        else if(!HasTargetsInRange() && _state == State.Stay)
        {
            statement = false;
            _state = State.Move;
           return unit.Pos;
            
        }
        else
        {
            statement = true;
            return base.GetNextStep();
        }
            
           
        
        


    }
    protected override List<Vector2Int> SelectTargets()
    {
        List<Vector2Int> nezen = new();
        var result = GetReachableTargets();
        if (_state == State.Stay && HasTargetsInRange())
        {
            _state = State.Shoot;
            statement = false;
            return nezen;
        }
        else if (_state == State.Shoot) 
        {
            _state = State.Stay;
            while (result.Count > 1)
                result.RemoveAt(result.Count - 1);
            return result;
        }
        return nezen;
        
    }
   
    
    // Update is called once per frame
    public override void Update(float deltaTime, float time)
    {
        
        if(statement == false)
        {
            sleep += Time.deltaTime;
            
        }
      
        if(statement == true)
        {
            sleep += Time.deltaTime;
        }
        if (sleep > awake)
        {

            sleep = 0f;

        }

    }
  
}
