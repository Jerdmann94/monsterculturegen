using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

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
    public CultureStartingData myStartingData;

    
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
        this.tilesThisCultureIsOn = new List<MapTile>();
        this.name = data.name +" "+ yearFounded;
        this.yearFounded = yearFounded;
        this.birthingAge = data.birthingAge;
        this.lifeSpan = data.lifeSpan;
        this.friends = new List<Culture>();
        this.enemies = new List<Culture>();
        this.peopleOfInterest = MakePeopleOfInterest(this);
        this.deadPeople = new List<PersonOfInterest>();
        this.totalPopulation = Random.Range(30, 100);
        this.food = Random.Range(60, 100);
        this.actions = MakeBasicActions();
        this.currentActionResource = 0;
        SetMapCulture(map[startingPosition[0],startingPosition[1]]);
        this.startingMapPosition = startingPosition;
        this.currentHappiness = 50;
        this.myStartingData = data;
        
//        Debug.Log("people of interest count " + peopleOfInterest.Count);
        //      Debug.Log("Made new culture with name " + this.name);
    }

    public void SetMapCulture(MapTile tile)
    {
        tile.culture = this;
        if (!tilesThisCultureIsOn.Contains(tile))
        {
            tilesThisCultureIsOn.Add(tile);
        }
        
    }
    public void RemoveMapCulture(MapTile tile)
    {
        tile.culture = null;
        tilesThisCultureIsOn.Remove(tile);
        this.totalPopulation =- tile.currentPopulation;
        tile.currentPopulation = 0;
    }

    public MapTile GetTileFromThisCulture()
    {
        Debug.Log(tilesThisCultureIsOn.Count);
        return tilesThisCultureIsOn.Count == 1 ? null : tilesThisCultureIsOn[Random.Range(0, tilesThisCultureIsOn.Count-1)];
    }
    public MapTile GetTileFromThisCultureAllowForLastTile()
    {
        Debug.Log(tilesThisCultureIsOn.Count);
        return tilesThisCultureIsOn[Random.Range(0, tilesThisCultureIsOn.Count-1)];
    }

    private void RecountPopulation()
    {
        var temp = 0;
        var tilesToBeRemoved = new List<MapTile>();
        foreach (var tile in tilesThisCultureIsOn)
        {
            if (tile.currentPopulation > tile.maxPopulation)
            {
                tile.currentPopulation = tile.maxPopulation;
            }

            if (tile.currentPopulation < 0)
            {
                tilesToBeRemoved.Add(tile);
                continue;
            }
            temp += tile.currentPopulation;
        }

        totalPopulation = temp;
        Debug.Log("Population Recount = "+totalPopulation + " for culture " + name);
        foreach (var VARIABLE in tilesToBeRemoved)
        {
            RemoveMapCulture(VARIABLE);
        }
    }

    private List<PersonOfInterest> MakePeopleOfInterest(Culture culture)
    {
        var people = new List<PersonOfInterest>();
        people.Add(new PersonOfInterest(culture.birthingAge+1,null,PersonOfInterestRole.King,culture,culture.GenerateName()));
        var rand = Random.Range(0, 5);
        for (int i = 0; i <= rand; i++)
        {
            people.Add(new PersonOfInterest(culture.birthingAge+1,null,PersonOfInterestRole.Advisor,culture,culture.GenerateName()));
        }
        
        return people;
    }

    private List<CultureAction> MakeBasicActions()
    {
        var list = new List<CultureAction>();
        list.Add(new CultureAction(3, "ExpandDomain",CultureActionType.Labor));
        list.Add(new CultureAction(2, "GrowPopulation",CultureActionType.Labor));
        list.Add(new CultureAction(5, "BuildMonument",CultureActionType.Happiness));
        list.Add(new CultureAction(1,"SowFields",CultureActionType.Food));
        //list.Add(new CultureAction(2,"DeclareWar"),CultureActionType.War);
        list.Add(new CultureAction(5, "Marriage", CultureActionType.Labor));
        list.Add(new CultureAction(2, "Envoy",CultureActionType.Happiness));
        return list;
    }

    public void DoAction(CultureAction chosenAction, List<Culture> culturesInvolved)
    {
        chosenAction.DoAction(culturesInvolved);
        RecountPopulation();
    }


    public void PrintInfo()
    {
        var info = " ";
        info += "Name: " + name + " ";
        info += "Year Founded: " + yearFounded + " ";
        info += "Total Population: " + totalPopulation + " ";
        info += "Friends: " + friends.Count + " ";
        info += "Enemies: " + enemies.Count + " ";
        info += GetPeopleOfInterestInfo(peopleOfInterest);
        info += "Dead People: " + deadPeople.Count + " ";
        info += "Birthing Age: " + birthingAge + " ";
        info += "Life Span: " + lifeSpan + " ";
        info += "Food: " + food + " ";
        info += "Actions: " + actions + " ";
        info += "Current Action Resource: " + currentActionResource + " ";
        info += "Starting Map Position: " + startingMapPosition + " ";
        info += "Tiles This Culture is On: " + tilesThisCultureIsOn.Count + " ";
        info += "Current Happiness: " + currentHappiness + " ";
        info += "My Starting Data: " + myStartingData;
        
        Debug.Log(info);
    }

    private string GetPeopleOfInterestInfo(List<PersonOfInterest> personOfInterests)
    {
        var info = "";
        foreach (var poi in personOfInterests)
        {
            info += "PERSON: ";
            info += "Name: " + poi.name + ", ";
            info += "Age: " + poi.age + ", ";
            info += "Spouse: " + (poi.spouse != null ? poi.spouse.name : "None") + ", ";
            info += "Role: " + poi.role + ", ";
            info += "Original Culture: " + (poi.originalCulture != null ? poi.originalCulture.ToString() : "None") + " ";;
        }
        return info;
    }

    public MapTile GetLowestFoodTile()
    {
        MapTile currentLowestFood = null;
        var currentFood = 100;
        foreach (var tile in tilesThisCultureIsOn)
        {
            if (currentFood > tile.currentFood)
            {
                currentLowestFood = tile;
            }
        }

        return currentLowestFood ??= tilesThisCultureIsOn[0];
    }

    public void KillPersonOfInterest(PersonOfInterest person)
    {
        deadPeople.Add(person);
        peopleOfInterest.Remove(person);
        if (person.role is not (PersonOfInterestRole.King or PersonOfInterestRole.Queen)) return;
        // Select the oldest person with a Royalty role
        var oldestRoyalty = peopleOfInterest
            .Where(p => p.role == PersonOfInterestRole.Royalty)
            .OrderByDescending(p => p.age)
            .FirstOrDefault();

        if (oldestRoyalty != null)
        {
            oldestRoyalty.role = person.role;
            return;
        }

        // If no one is Royalty, select a random person from the peopleOfInterest list
        if (peopleOfInterest.Any())
        {
            var oldest = peopleOfInterest[Random.Range(0, peopleOfInterest.Count)];
            oldest.role = person.role;
            return;
        }

        // If no one is in the peopleOfInterest list, create a new one
        var regex = new System.Text.RegularExpressions.Regex(@"(\d+)$");
        var match = regex.Match(person.name);

        string newName;
        if (match.Success)
        {
            // If an integer exists, increment it
            int currentValue = int.Parse(match.Value);
            newName = regex.Replace(person.name, (currentValue + 1).ToString());
        }
        else
        {
            // If no number exists at the end, append 1
            newName = person.name + "the 2";
        }

        var newPerson = new PersonOfInterest(
            Random.Range(0, person.originalCulture.lifeSpan), 
            null, 
            person.role,
            person.originalCulture, 
            newName
        );
        peopleOfInterest.Add(newPerson);
    }

    public string GenerateName()
    {
        return names[Random.Range(0, names.Count)] + " " + name;
    }

    public void AddFriend(Culture targetCulture)
    {
        if (targetCulture == null) return;
        if(friends.Contains(targetCulture)) return;
        friends.Add(targetCulture);
        targetCulture.AddFriend(this);
    }

    public void AddEnemy(Culture targetCulture)
    {
        if (targetCulture == null) return;
        if(enemies.Contains(targetCulture)) return;
        enemies.Add(targetCulture);
        targetCulture.AddEnemy(this);
    }
}