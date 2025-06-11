using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.WSA;
using Random = UnityEngine.Random;

public class CultureGenMaster
{
    private List<ACulturalEvent> events = new List<ACulturalEvent>();
    private List<Culture> _cultures = new List<Culture>();
    private List<CultureStartingData> _cultureStartingDatas = new List<CultureStartingData>();
    private int years = 1000;
    public MapTile[,] map;
    public static CultureGenMaster Instance;
    public int currentYear;
    public readonly int MAPSIZE = 100;
    public (MapTile[,], List<Culture>) Generate(List<ACulturalEvent> events, List<CultureStartingData> startingData)
    {
        Instance = this;
        map = new MapTile[MAPSIZE,MAPSIZE];
        for (int i = 0; i < MAPSIZE; i++)
        {
            for (int j = 0; j < MAPSIZE; j++)
            {
                map[i, j] = new MapTile(i,j);
            }
        }
        this._cultureStartingDatas = startingData;
        this.events = events;
        _cultures.Add(new Culture(_cultureStartingDatas[Random.Range(0, _cultureStartingDatas.Count)], 0,GetEmptyMapPosition(),map));
        _cultures.Add(new Culture(_cultureStartingDatas[Random.Range(0, _cultureStartingDatas.Count)], 0,GetEmptyMapPosition(),map));
        _cultures.Add(new Culture(_cultureStartingDatas[Random.Range(0, _cultureStartingDatas.Count)], 0,GetEmptyMapPosition(),map));
        for (currentYear = 1; currentYear <= years; currentYear++)
        {
            
            //DO CULTURE ACTIONS
            ResetCultureActionEconomy();
            var iterations = 0;
            //ROLL FOR NEW CULTURES
            if (RollForNewCulture(currentYear) is { } c)
            {
                _cultures.Add(c);
            }
            while (DoAnyCulturesHaveActionsAvailable())
            {
                foreach (var culture in _cultures)
                {
                    if (culture.currentActionResource > 0) MakeCultureActionChoice(culture);
                }
                if(iterations++ > 1000)
                {
                    Debug.Log("WHILE BREAKOUT HAPPENED AT CULTURE ACTIONS");
                    break;
                }
            }
            
            //REMOVING THESE EVENTS FOR NOW. GOING TO REPLACE THESE WITH ACTIONS
            //ROLL FOR CULTURE EVENTS
            foreach (var culture in _cultures)
            {
//                Debug.Log("Rolling for events for "+culture.name);
               // RollForEvents(culture);
            }

            DoYearlyDuties(_cultures);
            
            Debug.Log("One year has passed. "+currentYear);
            Debug.Log("Total number of cultures " + _cultures.Count);
        }

        foreach (var culture in _cultures)
        {
            culture.PrintInfo();
        }
        

        return (map, _cultures);
    }

    private void DoYearlyDuties(List<Culture> cultures)
    {
        CheckFoodHappinessRevoltsStarvation(cultures);
        AgePopulation(cultures);
        CheckForDeadCultures(cultures);
    }

    private void CheckForDeadCultures(List<Culture> cultures)
    {
        var toBeRemoved = new List<Culture>();
        foreach (var culture in _cultures)
        {
            if (culture.tilesThisCultureIsOn.Count == 0 ) toBeRemoved.Add(culture);
            if(culture.totalPopulation < 1) toBeRemoved.Add(culture);
        }

        foreach (var culture in toBeRemoved)
        {
            cultures.Remove(culture);

            // Remove culture from all associated tiles
            foreach (var tile in culture.tilesThisCultureIsOn.ToList()) // Use `ToList()` to safely iterate over a collection that is being modified
            {
                culture.RemoveMapCulture(tile);
            }

            Debug.Log($"{culture.name} has been killed at year "+currentYear+". Removed from Culture List");
        }
    }

    private void CheckFoodHappinessRevoltsStarvation(List<Culture> cultures)
    {
        foreach (var culture in cultures)
        {
            
            //UPDATE HAPINESS
            var monumentCount = 0;
            foreach (var maptile in culture.tilesThisCultureIsOn)
            {
               monumentCount += maptile.currentMonuments.Count;
                
            }
            culture.currentHappiness += monumentCount;
            if(culture.currentHappiness > 100) culture.currentHappiness = 100;
            //COUNT TOTAL FOOD
            var currentTotalFood = 0;
            foreach (var tile in culture.tilesThisCultureIsOn)
            {
                //tile.currentFood += tile.foodRegen;
                //MAX TILE FOOD IS EQUAL TO TILE REGEN
                if (tile.currentFood > tile.foodRegen) tile.currentFood = tile.foodRegen;
                currentTotalFood += tile.currentFood;
            }
            //CHECK FOR STARVATION
            if (currentTotalFood < culture.totalPopulation)
            {
                culture.currentHappiness -= 10;
                if (culture.currentHappiness < 0) culture.currentHappiness = 0;
                CheckForStarvation(culture);
            }
            //CHECK FOR REVOLTS
            if (culture.currentHappiness < 50)
            {
                CheckForRevolt(culture);
            }
        }
    }

    private void CheckForRevolt(Culture culture)
    {
        var revoltModifer = culture.currentHappiness > 30 ? -1 : -3;
        var dieResult = DiceRoll.Roll2d6();
        var totalResult = dieResult+revoltModifer;
        switch (totalResult)
        {
            case < 4: // Handles values less than 4
                Debug.Log("Severe Revolt for "+culture.name);
                DoRevolt(culture);
                DoRevolt(culture);
                DoRevolt(culture);
                break;
            case < 7: // Handles values between 3 (inclusive) and 8 (exclusive)
                Debug.Log("Revolt for "+culture.name);
                DoRevolt(culture);
                break;
            case >= 7:
                Debug.Log("Revolt Avoided for "+culture.name);
                break;
            default: // Handles all other values
                Console.WriteLine("Result is 8 or greater");
                break;
        }
    }

    private void DoRevolt(Culture culture)
    {
        if (culture.tilesThisCultureIsOn.Count < 2) return;
        var tileToRevolt = culture.GetTileFromThisCulture();
        culture.RemoveMapCulture(tileToRevolt);
        var newCulture = new Culture(culture.myStartingData,currentYear,new []{tileToRevolt.myX,tileToRevolt.myY},map);
        newCulture.enemies.Add(culture);
        culture.enemies.Add(newCulture);
    }

    private void CheckForStarvation(Culture culture)
    {
        var happinessModifer = culture.currentHappiness > 50 ? 1 : -1;
        var dieResult = DiceRoll.Roll2d6();
        var totalResult = dieResult+happinessModifer;
        switch (totalResult)
        {
            case < 4: // Handles values less than 4
                Debug.Log("Severe Starvation for "+culture.name);
                culture.currentHappiness -= 10;
                DoStarvationTileTax(5, culture);
                break;
            case >= 3 and < 7: // Handles values between 3 (inclusive) and 8 (exclusive)
                Debug.Log("Starvation for "+culture.name);
                culture.currentHappiness -= 5;
                DoStarvationTileTax(2, culture);
                break;
            case >= 7:
                Debug.Log("Starvation Avoided for "+culture.name);
                break;
            default: // Handles all other values
                Console.WriteLine("Result is 8 or greater");
                break;
        }
    }

    private void DoStarvationTileTax(int starvationValue, Culture culture)
    {
        var tilesToBeRemoved = new List<MapTile>();
        foreach (var tile in culture.tilesThisCultureIsOn)
        {
            tile.currentPopulation -= starvationValue;
            if (tile.currentPopulation < 0) tilesToBeRemoved.Add(tile);
        }

        var iterations = 0;
        while (tilesToBeRemoved.Count > 0)
        {
            culture.RemoveMapCulture(tilesToBeRemoved[0]);
            if(iterations++ > 1000)
            {
                Debug.Log("WHILE BREAKOUT HAPPENED AT TILES TO BE REMOVED");
                break;
            }
        }
        
    }

    private void MakeCultureActionChoice(Culture culture)
    {
        var possibleActions = new List<CultureAction>();

        foreach (var action in culture.actions)
        {
            if (action.cost <= culture.currentActionResource)
            {
                possibleActions.Add(action);
            }
        }

        var chosenAction = VoteForActionFromCulture(possibleActions,culture);
        if (chosenAction == null)
        {
            culture.currentActionResource--;
        }
        else
        {
            culture.DoAction(chosenAction,new List<Culture> { culture });
        }
       
        
        
    }

    private CultureAction VoteForActionFromCulture(List<CultureAction> possibleActions, Culture culture)
    {
        var people = culture.peopleOfInterest;
        var votePool = new List<CultureAction>();
        foreach (var person in people)
        {
            var votes = person.MakeVoteFromActions(possibleActions);
            foreach (var vote in votes)
            {
                votePool.Add(vote);
            }
        }
        
        // CHECK FOR A MAJORITY VOTE
        if (votePool.Count == 0) 
        {
            // If there are no votes, return a "chaos" action or handle the case accordingly.
            return null; // or your chosen "Chaos" indicator.
        }

        // Calculate the majority threshold
        //MAJORITY THRESHHOLD IS 25% OF VOTES
        int majorityThreshold = votePool.Count / 4;

        // Count votes using a dictionary
        var voteCounts = new Dictionary<CultureAction, int>();
        foreach (var vote in votePool)
        {
            if (voteCounts.ContainsKey(vote))
            {
                voteCounts[vote]++;
            }
            else
            {
                voteCounts[vote] = 1;
            }
        }

        // Find if there’s a majority
        foreach (var kvp in voteCounts)
        {
            if (kvp.Value > majorityThreshold)
            {
                return kvp.Key; // Return the action that has the majority
            }
        }

        // If no majority vote, chaos (or an action to indicate no consensus)
        Debug.Log("NO VOTE MAJORITY HAS REACHED, CHAOS RETURNED");
        return null; // or handle "chaos" logic here
    }

    private void ResetCultureActionEconomy()
    {
        foreach (var culture in _cultures)
        {
            culture.currentActionResource = culture.totalPopulation / 3;
        }
    }

    private bool DoAnyCulturesHaveActionsAvailable()
    {
        var listOfCulturesWithActionsLeft = new List<Culture>();
        foreach (var culture in _cultures)
        {
            if (culture.currentActionResource > 0)
            {
                listOfCulturesWithActionsLeft.Add(culture);
            }
        }
        return listOfCulturesWithActionsLeft.Count > 0;
    }

    private void AgePopulation(List<Culture> cultures)
    {
        foreach (var culture in _cultures)
        {
            var toRemove = new List<PersonOfInterest>();
            foreach (var person in culture.peopleOfInterest)
            {
                person.age++;
                if (person.age >= culture.lifeSpan)
                {
                    Debug.Log(person.name + " has died at " + person.age + " years old.");
        
                    // Mark person for removal
                    toRemove.Add(person);
                }
            }

// Remove marked people
            foreach (var person in toRemove)
            {
                culture.KillPersonOfInterest(person);
            }

            foreach (var person in culture.deadPeople)
            {
                if (culture.peopleOfInterest.Contains(person))
                {
                    culture.peopleOfInterest.Remove(person);
                }
            }
        }
    }
    

    Culture RollForNewCulture(int year)
    {
        var rand = Random.Range(0, 100);
        if (rand < 10)
        {
            var cultureData = _cultureStartingDatas[Random.Range(0, _cultureStartingDatas.Count-1)];
            Debug.Log("New Culture has formed "+cultureData.name);
            return new Culture(cultureData,year,GetEmptyMapPosition(),map);
        }

        return null;
    }

  

    private int[] GetEmptyMapPosition()
    {
        bool returnable = false;
        var coords = new int [2];
        var iterations = 0;
        while (returnable == false)
        {
            int var1 = Random.Range(0, MAPSIZE);
            int var2 = Random.Range(0, MAPSIZE);
            returnable = map[var1, var2].culture == null;
            coords = new[]{var1, var2};
            if(iterations++ > 1000)
            {
                Debug.Log("WHILE BREAKOUT HAPPENED AT GET EMPTY MAP");
                break;
            }
        }

        return coords;
    }

    private void RollForEvents(Culture culture)
    {
        foreach (ACulturalEvent cultureEvent in events)
        {
            var rand = Random.Range(0, 100);
            if (rand < cultureEvent.chanceIn100)
            {
                Debug.Log("Some Event Has Happened. " + cultureEvent.name +" for culture " + culture.name);
                cultureEvent.DoAction(new List<Culture> { culture });
                break;
            }
        }
    }

    public List<Culture> GetCultures()
    {
        return _cultures;
    }
}

public class MapTile
{
    public int myX;
    public int myY;
    public int currentFood;
    public int foodRegen;
    public int maxPopulation;
    public Culture culture;
    public int currentPopulation;
    public List<TileMonument> currentMonuments;

    public MapTile(int x, int y)
    {
        this.myX = x;
        this.myY = y;
        this.currentFood = Random.Range(20, 100);
        this.foodRegen = Random.Range(20, 100);
        this.maxPopulation = Random.Range(20, 100);
        this.currentPopulation = 0;
        this.culture = null;
        currentMonuments = new List<TileMonument>();
    }

}

public class TileMonument
{
    public string name;
    public MapTile tile;

    public TileMonument(MapTile tile)
    {
        this.tile = tile;
        name = "NEW MONUMENT";
    }
    
}






/*
 DICE STATISTICS FOR 2D6 RESULTS
 * Dice Score	Result exactly	Result or less	Result or more
    2	            2.77	        2.77	    100
    3	            5.55	        8.33	    97.22
    4	            8.33	        16.66	    91.66
    5	            11.11	        27.77	    83.33
    6	            13.88	        41.66	    72.22
    7	            16.66	        58.33	    58.33
    8	            13.88	        72.22	    41.66
    9	            11.11	        83.33	    27.77
    10	            8.33	        91.66	    16.66
    11	            5.55	        97.22	    8.33
    12	            2.77	        100	        2.77
 */
public static class DiceRoll
{
    public static int Roll2d6()
    {
        System.Random random = new System.Random();
        int die1 = random.Next(1, 7); // Generates a number between 1 and 6
        int die2 = random.Next(1, 7); // Generates a number between 1 and 6
        return die1 + die2; // Returns the sum of both dice
    }
}

