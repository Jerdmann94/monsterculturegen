using System;
using System.Collections.Generic;
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
    private int years = 100;
    public MapTile[,] map;
    public static CultureGenMaster Instance;

    public void Generate(List<ACulturalEvent> events, List<CultureStartingData> startingData)
    {
        Instance = this;
        map = new MapTile[100,100];
        for (int i = 0; i < 100; i++)
        {
            for (int j = 0; j < 100; j++)
            {
                map[i, j] = new MapTile(i,j);
            }
        }
        this._cultureStartingDatas = startingData;
        this.events = events;
        _cultures.Add(new Culture(_cultureStartingDatas[Random.Range(0, _cultureStartingDatas.Count)], 0,GetEmptyMapPosition(),map));
        _cultures.Add(new Culture(_cultureStartingDatas[Random.Range(0, _cultureStartingDatas.Count)], 0,GetEmptyMapPosition(),map));
        _cultures.Add(new Culture(_cultureStartingDatas[Random.Range(0, _cultureStartingDatas.Count)], 0,GetEmptyMapPosition(),map));
        for (int year = 1; year <= years; year++)
        {
            
            //DO CULTURE ACTIONS
            ResetCultureActionEconomy();
            while (DoAnyCulturesHaveActionsAvailable())
            {
                foreach (var culture in _cultures)
                {
                    if (culture.currentActionResource > 0) MakeCultureActionChoice(culture);
                }
            }
//            Debug.Log("current culture count " + _cultures.Count);

            //ROLL FOR NEW CULTURES
            if (RollForNewCulture(year) is { } c)
            {
                _cultures.Add(c);
            }
            //ROLL FOR CULTURE EVENTS
            foreach (var culture in _cultures)
            {
//                Debug.Log("Rolling for events for "+culture.name);
                RollForEvents(culture);
            }

            DoYearlyDuties(_cultures);
            
            Debug.Log("One year has passed. "+year);
            Debug.Log("Total number of cultures " + _cultures.Count);
        }
    }

    private void DoYearlyDuties(List<Culture> cultures)
    {
        
        CheckFoodHappinessRevoltsStarvation(cultures);
        AgePopulation(cultures);
    }

    private void CheckFoodHappinessRevoltsStarvation(List<Culture> cultures)
    {
        foreach (var culture in cultures)
        {
            //COUNT TOTAL FOOD
            var currentTotalFood = 0;
            foreach (var tile in culture.tilesThisCultureIsOn)
            {
                tile.currentFood += tile.foodRegen;
                //ADDING ARBITRARY TOTAL TILE FOOD OF 200 FOR NOW
                if (tile.currentFood > 200) tile.currentFood = 200;
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
            
        }
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
            case >= 3 and < 8: // Handles values between 3 (inclusive) and 8 (exclusive)
                Debug.Log("Starvation for "+culture.name);
                culture.currentHappiness -= 5;
                DoStarvationTileTax(2, culture);
                break;
            case >= 8:
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

        while (tilesToBeRemoved.Count > 0)
        {
            culture.RemoveMapCulture(tilesToBeRemoved[0]);
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
        var chosenAction = possibleActions[Random.Range(0, possibleActions.Count)];
        culture.DoAction(chosenAction,new List<Culture> { culture });
        
        
    }

    private void ResetCultureActionEconomy()
    {
        foreach (var culture in _cultures)
        {
            culture.currentActionResource = culture.totalPopulation / 10;
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
            foreach (var person in culture.peopleOfInterest)
            {
                person.age++;
                if (person.age >= culture.lifeSpan)
                {
                    Debug.Log(person.name + " has died at " + person.age + " years old.");
                    culture.deadPeople.Add(person);
                        
                }
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
        while (returnable == false)
        {
            int var1 = Random.Range(0, 100);
            int var2 = Random.Range(0, 100);
            returnable = map[var1, var2].culture == null;
            coords = new[]{var1, var2};
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

    public MapTile(int x, int y)
    {
        this.myX = x;
        this.myY = y;
        this.currentFood = Random.Range(0, 100);
        this.foodRegen = Random.Range(0, 100);
        this.maxPopulation = Random.Range(40, 100);
        this.currentPopulation = 0;
        this.culture = null;
    }
}

public class PersonOfInterest
{
    public int age;
    public PersonOfInterest spouse = null;
    public PersonOfInterestRole role = PersonOfInterestRole.King;
    public Culture originalCulture;
    public string name;
    public PersonOfInterest(int age, PersonOfInterest spouse, PersonOfInterestRole role,Culture culture,string name)
    {
        this.age = age;
        this.spouse = spouse;
        this.role = role;
        this.originalCulture = culture;
        this.name = name;
    }
    
}

public enum PersonOfInterestRole
{
    King,
    Queen,
    Royalty,
    Advisor,
    Diplomat,
}


public class Culture
{
    public string name;
    public int yearFounded;
    public int totalPopulation;
    public List<Culture> friends;
    public List<Culture> enemies;
    public List<PersonOfInterest> peopleOfInterest;
    public List<PersonOfInterest> deadPeople;
    public int birthingAge;
    public int lifeSpan;
    public int food;
    public List<CultureAction> actions;
    public int currentActionResource;
    public int[] startingMapPosition;
    public List<MapTile> tilesThisCultureIsOn;
    public int currentHappiness;

    
    List<string> names = new List<string>
    {
        "Alice", "Bob", "Charlie", "David", "Eve", "Frank", "Grace", "Hannah", "Isaac", "Jack",
        "Karen", "Liam", "Mia", "Nathan", "Olivia", "Paul", "Quinn", "Rachel", "Sam", "Tina",
        "Uma", "Victor", "Wendy", "Xander", "Yara", "Zane", "Amber", "Brian", "Cathy", "Derek",
        "Elena", "Fred", "Gina", "Harry", "Ivy", "Jon", "Katie", "Leo", "Megan", "Noah",
        "Opal", "Peter", "Queen", "Riley", "Scott", "Tara", "Ulysses", "Violet", "Will", "Xenia",
        "Yusuf", "Zoe", "Annie", "Ben", "Carla", "Dylan", "Emily", "Felix", "Georgia", "Hector",
        "Isla", "Jake", "Kara", "Luca", "Molly", "Nick", "Owen", "Penny", "Quincy", "Rose",
        "Shawn", "Tracy", "Uri", "Val", "Wanda", "Ximena", "Yvette", "Zack", "April", "Brent",
        "Carmen", "Dean", "Elsa", "Finn", "Gabby", "Haley", "Ian", "Jill", "Kyle", "Lily",
        "Marcus", "Nina", "Oscar", "Paula", "Reed", "Sophie", "Tom", "Tiffany", "Vera", "Wes"
    };

   


    public Culture(CultureStartingData data, int yearFounded,int[] startingPosition,MapTile[,] map)
    {
        this.name = data.name +" "+ yearFounded;
        this.yearFounded = yearFounded;
        this.birthingAge = data.birthingAge;
        this.lifeSpan = data.lifeSpan;
        this.friends = new List<Culture>();
        this.enemies = new List<Culture>();
        this.peopleOfInterest = MakePeopleOfInterest(this);
        this.deadPeople = new List<PersonOfInterest>();
        this.totalPopulation = Random.Range(10, 100);
        this.food = Random.Range(10, 100);
        this.actions = MakeBasicActions();
        this.currentActionResource = 0;
        SetMapCulture(map[startingPosition[0],startingPosition[1]]);
        this.startingMapPosition = startingPosition;
        this.currentHappiness = 50;
//        Debug.Log("people of interest count " + peopleOfInterest.Count);
        //      Debug.Log("Made new culture with name " + this.name);
    }

    public void SetMapCulture(MapTile tile)
    {
        tile.culture = this;
        tilesThisCultureIsOn.Add(tile);
    }
    public void RemoveMapCulture(MapTile tile)
    {
        tile.culture = null;
        tilesThisCultureIsOn.Remove(tile);
        tile.currentPopulation = 0;
    }

    private void RecountPopulation()
    {
        var temp = 0;
        foreach (var tile in tilesThisCultureIsOn)
        {
            if (tile.currentPopulation > tile.maxPopulation)
            {
                tile.currentPopulation = tile.maxPopulation;
            }
            temp += tile.currentPopulation;
        }
    }

    private List<PersonOfInterest> MakePeopleOfInterest(Culture culture)
    {
        var people = new List<PersonOfInterest>();
        people.Add(new PersonOfInterest(culture.birthingAge+1,null,PersonOfInterestRole.King,culture,names[Random.Range(0,names.Count)] +" "+ culture.name));
        var rand = Random.Range(0, 5);
        for (int i = 0; i <= rand; i++)
        {
            people.Add(new PersonOfInterest(culture.birthingAge+1,null,PersonOfInterestRole.Advisor,culture,names[Random.Range(0,names.Count)] +" "+ culture.name));
        }
        
        return people;
    }

    private List<CultureAction> MakeBasicActions()
    {
        var list = new List<CultureAction>();
        list.Add(new CultureAction(3, "ExpandDomain"));
        list.Add(new CultureAction(2, "GrowPopulation"));
        list.Add(new CultureAction(5, "BuildMonument"));
        list.Add(new CultureAction(1,"SowFields"));
        list.Add(new CultureAction(2,"DeclareWar"));
        return list;
    }

    public void DoAction(CultureAction choosenAction, List<Culture> culturesInvolved)
    {
        choosenAction.DoAction(culturesInvolved);
        RecountPopulation();
    }

    
}

public class CultureAction
{
    public int cost;
    public string name;

    public CultureAction(int cost, string name)
    {
        this.cost = cost;
        this.name = name;
    }

  
    public void DoAction(List<Culture> cultures)
    {
        
        var culture = cultures[0];
        culture.currentActionResource -= cost;
        switch (name)
        {
            case "ExpandDomain":
                Debug.Log("ExpandDomain");
                var testTile = culture.tilesThisCultureIsOn[Random.Range(0, culture.tilesThisCultureIsOn.Count)];
                var targetTile = GetEmptyTileAdjacentToThisTile(testTile);
                if (targetTile != null)
                {
                    culture.SetMapCulture(targetTile);
                    var halfPop = testTile.currentPopulation / 2;
                    targetTile.currentPopulation = halfPop;
                    testTile.currentPopulation = halfPop;
                }
                break;
            case "GrowPopulation":
                foreach (var tile in culture.tilesThisCultureIsOn)
                {
                    if(tile.currentPopulation >= tile.maxPopulation) continue;
                    culture.food -= tile.currentPopulation/2;
                    if (culture.food < 0)
                    {
                        culture.food = 0;
                        Debug.Log(culture.name + " ran out of food while growing population.");
                        break;
                    }
                    tile.currentPopulation += 5;
                    if(tile.currentPopulation >= tile.maxPopulation) tile.currentPopulation = tile.maxPopulation;
                }
                Debug.Log("GrowPopulation");
                break;
            case "BuildMonument":
                Debug.Log("BuildMonument");
                break;
            case "SowFields":
                Debug.Log("SowFields");
                break;
            case "DeclareWar":
                Debug.Log("DeclareWar");
                break;
            case "Marriage":
                break;
            default:
                break;
        }
    }

    private MapTile GetEmptyTileAdjacentToThisTile(MapTile testTile)
    {
        var tiles = new List<MapTile>();
        var map = CultureGenMaster.Instance.map;
        if(testTile.myX+1 < map.Length)
        {
            if (map[testTile.myX+1, testTile.myY] == null)
            {
                tiles.Add(new MapTile(testTile.myX+1, testTile.myY));   
            }
        }
        if(testTile.myX-1 > -1)
        {
            if (map[testTile.myX-1, testTile.myY] == null)
            {
                tiles.Add(new MapTile(testTile.myX-1, testTile.myY));   
                
            }
        }
        if(testTile.myY+1 < map.Length)
        {
            if (map[testTile.myX, testTile.myY+1] == null)
            {
                tiles.Add(new MapTile(testTile.myX, testTile.myY+1));   
            }
        }
        if(testTile.myY-1 > -1)
        {
            if (map[testTile.myX, testTile.myY-1] == null)
            {
                tiles.Add(new MapTile(testTile.myX, testTile.myY-1));   
            }
        }

        return tiles.Count == 0 ? null : tiles[Random.Range(0, tiles.Count)];
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

