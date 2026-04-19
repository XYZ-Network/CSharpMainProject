using Model;
using Model.Runtime.ReadOnly;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Commander : MonoBehaviour
{
    private IReadOnlyRuntimeModel _runtimeModel;

    public Vector2? RecommendTarget;
    public Vector2? RecommendPoint;


    public void Init(IReadOnlyRuntimeModel model)
    {
        _runtimeModel = model;
    }
    public Commander() { }

   

    private void Update()
    {
        if (_runtimeModel == null ||
           _runtimeModel.RoMap == null ||
           _runtimeModel.RoMap.Bases == null)
            return;

        Vector2 basePos = _runtimeModel.RoMap.Bases[RuntimeModel.PlayerId];

        List<IReadOnlyUnit> enimes = _runtimeModel.RoBotUnits.ToList();

        if (enimes.Count == 0)
        {
            RecommendTarget = null;
            RecommendPoint = null;
            return;

        }

        int ourHalf = _runtimeModel.RoMap.Height / 2;

        bool enemyOnOurSide = false;
        foreach (var enemy in enimes)
        {
            if (enemy.Pos.y < ourHalf)
            {
                enemyOnOurSide = true;
                break;
            }
        }
        if (enemyOnOurSide)
        {
            IReadOnlyUnit closest = null;
            float minDistance = float.MaxValue;

            foreach (var enemy in enimes)
            {
                float dist = Vector2.Distance(enemy.Pos, basePos);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closest = enemy;
                }
            }

            if (closest != null)
            {
                RecommendTarget = closest.Pos;
                RecommendPoint = basePos + new Vector2Int(0, 1);

            }
        }

        else
        {
            IReadOnlyUnit weakest = null;

            float minHealt = float.MaxValue;

            foreach (var enemy in enimes)
            {
                if (enemy.Health < minHealt)
                {
                    minHealt = enemy.Health;
                    weakest = enemy;
                }
            }
            if(weakest != null)
            {
                RecommendTarget = weakest.Pos;
                Vector2 dirToBase = basePos - weakest.Pos;
                Vector2 simpleDir = new Vector2(
                    dirToBase.x == 0 ? 0 : (dirToBase.x > 0 ? 1 : -1),
                        dirToBase.y == 0 ? 0 : (dirToBase.y > 0 ? 1 : -1)
                    );
                RecommendPoint = weakest.Pos + simpleDir * 2;

            }
        }
    }


}
        
   
   