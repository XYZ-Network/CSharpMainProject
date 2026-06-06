using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Model;
using Model.Runtime;

public interface IApplicable
{
    bool TryApplicate(Unit unit);
}
