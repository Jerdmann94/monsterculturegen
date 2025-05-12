using System.Collections.Generic;
using UnityEngine;

public class GameMaster : MonoBehaviour
{
    public List<ACulturalEvent> events = new List<ACulturalEvent>();
    public List<CultureStartingData> startingData = new List<CultureStartingData>();
    void Start()
    {
        var generator = new CultureGenMaster();
        generator.Generate(events,startingData);
    }

}
