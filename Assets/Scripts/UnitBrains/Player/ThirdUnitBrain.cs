using System.Collections;
using System.Collections.Generic;
using UnitBrains.Player;
using UnityEngine;

public class UnitControl : MonoBehaviour
{
    private enum UnitMode { Attacking, Moving }
    private ThirdUnitBrain unitBrain;
    private Vector2Int targetPosition;

    
    void Start()
    {
        unitBrain = GetComponent<ThirdUnitBrain>();
    }

    void Update()
    {
        if (unitBrain.GetCurrentMode() == UnitMode.Moving)
        {
            MoveUnit();
        }
        else if (unitBrain.GetCurrentMode() == UnitMode.Attacking)
        {
            AttackUnit();
        }
    }

    void MoveUnit()
    {
        float step = 2f * Time.deltaTime; // adjust speed as needed
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, step);
    }

    void AttackUnit()
    {
        // implement attack logic here
        Debug.Log("Attacking enemy unit.");
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            targetPosition = other.transform.position;
            unitBrain.SetCurrentMode(UnitMode.Attacking);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            targetPosition = transform.position;
            unitBrain.SetCurrentMode(UnitMode.Moving);
        }
    }
}


