using Model;
using Model.Runtime.Projectiles;
using System.Collections;
using System.Collections.Generic;
using UnitBrains.Pathfinding;
using UnitBrains.Player;
using UnityEngine;
using Utilities;

public class ThirdUnitBrain : DefaultPlayerUnitBrain
{
    private bool modeMoving = true;
    private float chageModeTimer;
    public override string TargetUnitName => "Ironclad Behemoth";

    protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
    {
        //Если сейчас режим движения, то не стреляем, отключаем движение и включаем таймер переключения
        if (modeMoving)
        {
            modeMoving = false;
            chageModeTimer = 0.3f;
            return;
        }
        else if(chageModeTimer > 0) //если таймер не нулевой - то отсчитываем, иначе можно стрелять
        {
            chageModeTimer -= Time.deltaTime;
        }
        else
        {
            base.GenerateProjectiles(forTarget, intoList);
        }
    }
    public override Vector2Int GetNextStep()
    {
        if (HasTargetsInRange()) //если есть в кого стрелять - то отрабатываем базовый метод
        {
            return base.GetNextStep();
        }
        else //иначе, надо двигаться и делаем задержку на случай если мы были в режиме атаки
        {
            if(!modeMoving) { //Если движение было отключено - то стоим на месте 1000 мс
                modeMoving =true;
                chageModeTimer = 0.3f;
                return unit.Pos;
            }
            else if (chageModeTimer > 0) //ждем 1000 мс
            {
                chageModeTimer -= Time.deltaTime;
                return unit.Pos;
            }
            else //если мод движения включен и таймер переключения = 0 - то можно ехать
            {
                return base.GetNextStep();
            }

        }

    }

  

}
