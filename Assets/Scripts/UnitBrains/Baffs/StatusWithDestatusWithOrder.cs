using System;
using System.Collections.Generic;
using System.Linq;
using Codice.Client.Common.GameUI;
using Model;
using Model.Runtime.Projectiles;
using UnityEngine;
using Utilities;
using UnitBrains;
using System.Collections;

namespace Baffs
{
    public struct StatusWithDestatusWithOrder
    {
        public Action<BaseUnitBrain> Status;
        public Action<BaseUnitBrain> Destatus;
        public int Order;

        public StatusWithDestatusWithOrder(Action<BaseUnitBrain> status, Action<BaseUnitBrain> destatus, int order)
        {
            Status = status;
            Destatus = destatus;
            Order = order;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            StatusWithDestatusWithOrder other = (StatusWithDestatusWithOrder)obj;
            return Status == other.Status &&
                   Destatus == other.Destatus &&
                   Order == other.Order;
        }
    }
}
