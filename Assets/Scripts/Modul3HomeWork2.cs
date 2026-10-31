using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        int Damage = Random.Range(5,30);
        int Shield = 15;
        int chanse = Random.Range(1,100);

        if (chanse > 25) {
            if (Damage > Shield)
            {
                int actualDamage = (Damage - Shield);
                Debug.Log("Игрок нанес" + actualDamage + "урон по цели");
            }
            else if (Damage <= Shield)
            {
                int absorbedDamage = Damage;
                Debug.Log("Щит поглотил" + absorbedDamage + "урона");
            }
        }
        else {
            Debug.Log("Промах");
        }
    }

}
