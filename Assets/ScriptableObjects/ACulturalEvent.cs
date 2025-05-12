using System.Collections.Generic;
using UnityEngine;


public abstract class ACulturalEvent : ScriptableObject
{
    public int chanceIn100;
    public Culture[] positiveCultures;
    public Culture[] negativeCultures;
    public abstract void DoAction(List<Culture> cultures);
}
