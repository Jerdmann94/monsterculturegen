using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CultureGenMaster
{
    private List<ACulturalEvent> events = new List<ACulturalEvent>();
    private List<Culture> _cultures = new List<Culture>();
    private List<CultureStartingData> _cultureStartingDatas = new List<CultureStartingData>();
    private int years = 100;

    public void Generate(List<ACulturalEvent> events, List<CultureStartingData> startingData)
    {
        this._cultureStartingDatas = startingData;
        this.events = events;
        _cultures.Add(new Culture(_cultureStartingDatas[Random.Range(0, _cultureStartingDatas.Count)], 0));
        _cultures.Add(new Culture(_cultureStartingDatas[Random.Range(0, _cultureStartingDatas.Count)], 0));
        _cultures.Add(new Culture(_cultureStartingDatas[Random.Range(0, _cultureStartingDatas.Count)], 0));
        for (int year = 1; year <= years; year++)
        {
//            Debug.Log("current culture count " + _cultures.Count);
            if (RollForNewCulture(year) is { } c)
            {
                _cultures.Add(c);
            }
            foreach (var culture in _cultures)
            {
//                Debug.Log("Rolling for events for "+culture.name);
                RollForEvents(culture);
            }

            AgePopulation(_cultures);
            Debug.Log("One year has passed. "+year);
            Debug.Log("Total number of cultures " + _cultures.Count);
        }
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
            return new Culture(cultureData,year);
        }

        return null;
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
    public int population;
    public List<Culture> friends;
    public List<Culture> enemies;
    public List<PersonOfInterest> peopleOfInterest;
    public List<PersonOfInterest> deadPeople;
    public int birthingAge;
    public int lifeSpan;

    
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

    


    public Culture(CultureStartingData data, int yearFounded)
    {
        this.name = data.name +" "+ yearFounded;
        this.yearFounded = yearFounded;
        this.birthingAge = data.birthingAge;
        this.lifeSpan = data.lifeSpan;
        this.population = 100;
        this.friends = new List<Culture>();
        this.enemies = new List<Culture>();
        this.peopleOfInterest = MakePeopleOfInterest(this);
        this.deadPeople = new List<PersonOfInterest>();
//        Debug.Log("people of interest count " + peopleOfInterest.Count);
  //      Debug.Log("Made new culture with name " + this.name);
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
}





