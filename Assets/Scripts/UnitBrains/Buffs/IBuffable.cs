using System.Collections;
using System.Collections.Generic;
using UnitBrains;
using UnityEngine;

// Done
public interface IBuffable
{
    public void AddStats(string Id, CharacterStats stats);
    public void ResetStats(string Id);
}
