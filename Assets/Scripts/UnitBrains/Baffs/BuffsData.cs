using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnitBrains;
using System.Linq;
using Codice.CM.Common.Serialization;

namespace Baffs
{
    public static class BuffsData
    {
        static Dictionary<BaseUnitBrain, List<List<BaseStatusEffect>>> Data = new Dictionary<BaseUnitBrain, List<List<BaseStatusEffect>>>();
        static Dictionary<BaseUnitBrain, bool> IsStatusesCorrect = new Dictionary<BaseUnitBrain, bool>();

        static public void Clear(int index = 2)
        {
            List<BaseUnitBrain> brains = Data.Keys.ToList();
            foreach (BaseUnitBrain brain in brains)
            {
                Data[brain][index] = Data[brain][0];
                IsStatusesCorrect[brain] = false;
            }
            UpdateStatuses();
        }

        static public bool TryAddStatusToData(BaseUnitBrain unit, BaseStatusEffect buff, int index = 1)
        {
            if (!Data.Keys.Any(u => u == unit))
            {
                Data.Add(unit, new List<List<BaseStatusEffect>>() { new List<BaseStatusEffect>() , new List<BaseStatusEffect>(), new List<BaseStatusEffect>() });
            }
            if (buff.CanAddStatusToData(Data[unit]))
            {

                StatusWithDestatusWithOrder buffData = new StatusWithDestatusWithOrder(buff.Effect, buff.Diseffect, buff.Order);

                Data[unit][index].Add(buff);
                IsStatusesCorrect[unit] = false;

                return true;
            }
            return false;
        }
        static public void DeleteStatusFromData(BaseUnitBrain unit, BaseStatusEffect buff, int index = 2)
        {
            StatusWithDestatusWithOrder buffData = new StatusWithDestatusWithOrder(buff.Effect, buff.Diseffect, buff.Order);

            Data[unit][index].Add(buff);
            IsStatusesCorrect[unit] = false;
        }

        static bool TryEnableEffectsToBrain(BaseUnitBrain brain, int index = 0)
        {
            foreach (BaseStatusEffect status in Data[brain][index])
            {
                if (!brain.TryApplyEffect(status))
                {
                    Debug.LogError($"Cant apply effect {status} to {brain}");
                    return false;
                }
            }
            return true;
        }

        static bool TryDisableEffectsFromBrain(BaseUnitBrain brain, int index = 0)
        {
            foreach (BaseStatusEffect status in Data[brain][index])
            {
                if (!brain.TryApplyEffect(status))
                {
                    Debug.LogError($"Cant disable effect {status} from {brain}");
                    return false;
                }
            }
            return true;
        }

        static void UpdateStatuses()
        {
            var brainsToProcess = IsStatusesCorrect.Keys.ToList();

            foreach (BaseUnitBrain brain in brainsToProcess)
            {
                if (!IsStatusesCorrect[brain])
                {
                    TryDisableEffectsFromBrain(brain);

                    Data[brain][0].AddRange(Data[brain][1]);
                    Data[brain][1].Clear();

                    List<BaseStatusEffect> statuses = Data[brain][2];

                    foreach (BaseStatusEffect status in statuses.ToList())
                    {
                        if (Data[brain][0].Any(u => u.Equals(status)))
                        {
                            Data[brain][0].Remove(status);
                        }
                        else
                        {
                            Debug.LogWarning($"Не получилось удалить {status} из {brain}");
                        }
                    }

                    Data[brain][2].Clear();

                    Data[brain][0].Sort((a, b) => a.Order.CompareTo(b.Order));
                    TryEnableEffectsToBrain(brain);
                    IsStatusesCorrect[brain] = true;
                }
            }
        }

        public static void Update(float deltaTime)
        {
            UpdateStatuses();
        }
    }
}
