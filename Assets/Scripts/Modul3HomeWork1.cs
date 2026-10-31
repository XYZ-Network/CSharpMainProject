using System.Collections;
using System.Collections.Generic;
using UnityEditor.VersionControl;
using UnityEngine;

public class Level2 : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(DamageUnit(12, 4));
        Debug.Log(DamageEnemy(9,3));
    }

      int DamageUnit(int MainDamage, int SingleDamage)
    {
        int MaxDamage = MainDamage + SingleDamage;
        return MaxDamage;
    }
    int DamageEnemy(int MainDamageEnemy, int SingleDamageEnemy)
    {
        int MaxDamageEnemy = MainDamageEnemy - SingleDamageEnemy;
        return MaxDamageEnemy;
    }
}
