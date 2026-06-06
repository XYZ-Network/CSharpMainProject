using System.Collections.Generic;
using System.Linq;
using Codice.Client.Common.GameUI;
using Model;
using Model.Runtime.Projectiles;
using UnityEngine;
using Utilities;


public struct PositionWithDanger
{
    public Vector2Int Position;
    public int Danger;

    public PositionWithDanger(Vector2Int position, int danger)
    {
        Position = position;
        Danger = danger;
    }
}
