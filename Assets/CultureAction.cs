using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CultureAction
{
    public int cost;
    public string name;
    public CultureActionType type;

    public CultureAction(int cost, string name,CultureActionType actionType)
    {
        this.cost = cost;
        this.name = name;
        this.type = actionType;
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
                Debug.Log("Target Tile: " + targetTile);
                if (targetTile != null)
                {
                    culture.SetMapCulture(targetTile);
                    var halfPop = testTile.currentPopulation / 2;
                    targetTile.currentPopulation = halfPop;
                    testTile.currentPopulation = halfPop;
                    Debug.Log("Successfully placed tile expansion");
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
                var monumentTile = culture.GetTileFromThisCultureAllowForLastTile();
                Debug.Log(monumentTile);
                monumentTile.currentMonuments.Add(new TileMonument(monumentTile));
                Debug.Log("BuildMonument");
                break;
            case "SowFields":
                var foodTile = culture.GetLowestFoodTile();
                Debug.Log(foodTile);
                foodTile.currentFood += foodTile.foodRegen;
                if(foodTile.currentFood > foodTile.foodRegen) foodTile.currentFood = foodTile.foodRegen;
                Debug.Log("SowFields");
                break;
            case "DeclareWar":
                Debug.Log("DeclareWar");
                break;
            case "Marriage":
                PersonOfInterest toBeMarriedTarget = null;
                var suitors = new List<PersonOfInterest>();
                foreach (var person in culture.peopleOfInterest)
                {
                    if (person.spouse != null) continue;
                    toBeMarriedTarget = person;
                    break;
                }
                if (culture.friends.Count > 0)
                {//GENERATE PARTNERS FROM OTHER CULTURES
                    foreach (var cultureFriend in culture.friends)
                    {
                       var p = GeneratePartners(cultureFriend, toBeMarriedTarget);
                       foreach (var person in p)
                       {
                           suitors.Add(person);
                       }
                    }
                }
                var p2 = GeneratePartners(culture,toBeMarriedTarget);
                foreach (var person in p2)
                {
                    suitors.Add(person);
                }

                
                if (suitors.Count == 0 || toBeMarriedTarget == null)
                {
                    Debug.Log("Marriage has failed for some unkown reason");
                    break;
                }
                var suitor = suitors[Random.Range(0, suitors.Count)];
                if (toBeMarriedTarget.role == PersonOfInterestRole.King)
                {
                    suitor.GiveRole(PersonOfInterestRole.Queen);
                }

                if (toBeMarriedTarget.role == PersonOfInterestRole.Queen)
                {
                    suitor.GiveRole(PersonOfInterestRole.King);
                }
                
                
                break;
            case "Envoy":
                Debug.Log("Envoy");
                //DISCOVER OTHER CULTURES
                var expiditionTile = culture.GetTileFromThisCultureAllowForLastTile();
                //Debug.Log(expiditionTile);
                expiditionTile.currentFood = 0;
                var nearestCulture = FindNearestCultureNotOnFriendsOrEnemies(culture, expiditionTile);
                if (nearestCulture == null)
                {
                    Debug.Log("Envoy has failed to find nearest culture");
                    return;
                }
                var result = DiceRoll.Roll2d6();
                if (result < 7)
                {
                    culture.AddEnemy(nearestCulture);
                }
                else
                {
                    culture.AddFriend(nearestCulture);
                }
                break;
            default:
                break;
                }
        }

    private Culture FindNearestCultureNotOnFriendsOrEnemies(Culture culture, MapTile expeditionTile)
    {
        var map = CultureGenMaster.Instance.map;
        Culture nearestCulture = null;
        float shortestDistance = float.MaxValue;

        // Get expedition tile's coordinates
        int expeditionX = expeditionTile.myX;
        int expeditionY = expeditionTile.myY;
        var allCultures = CultureGenMaster.Instance.GetCultures();
        // Iterate through all tiles in the map
        for (int x = 0; x < allCultures.Count-1; x++) // Assuming map.Width gives the number of columns
        {
            
            var testCulture = allCultures[Random.Range(0, allCultures.Count)];
            var currentTile = testCulture.GetTileFromThisCultureAllowForLastTile();
            // Skip if the current tile has no culture
                if (currentTile.culture == culture || expeditionTile == currentTile || expeditionTile.culture == null)
                    continue;

                // Calculate distance between expeditionTile and currentTile
                float distance = Mathf.Sqrt(
                    Mathf.Pow(currentTile.myX - expeditionX, 2) +
                    Mathf.Pow(currentTile.myY - expeditionY, 2)
                );

                // If current distance is closer, update the nearest culture
                Debug.Log("Envoy check = "+(distance < shortestDistance) );
                //if (distance < shortestDistance &&  !culture.friends.Contains(currentTile.culture) && !culture.enemies.Contains(currentTile.culture))
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    nearestCulture = currentTile.culture;
                }
            
        }

        return nearestCulture ?? null;
    }


    private List<PersonOfInterest> GeneratePartners(Culture culture,PersonOfInterest futureSpouse)
    {
        var suitorNum = Random.Range(1, 4);
        var suitors = new List<PersonOfInterest>();
        for (int i = 0; i < suitorNum; i++)
        {
            suitors.Add(new PersonOfInterest(Random.Range(culture.birthingAge,culture.lifeSpan),null,PersonOfInterestRole.Royalty,culture,culture.GenerateName()));
        }

        return suitors;
    }

    private MapTile GetEmptyTileAdjacentToThisTile(MapTile testTile)
    {
        var tiles = new List<MapTile>();
        var map = CultureGenMaster.Instance.map;
        if(testTile.myX+1 < CultureGenMaster.Instance.MAPSIZE)
        {
            if (map[testTile.myX+1, testTile.myY] != null)
            {
                tiles.Add(map[testTile.myX+1, testTile.myY]);   
            }
        }
        if(testTile.myX-1 > -1)
        {
            if (map[testTile.myX-1, testTile.myY] != null)
            {
                tiles.Add(map[testTile.myX-1, testTile.myY]);   
                
            }
        }
        if(testTile.myY+1 < CultureGenMaster.Instance.MAPSIZE)
        {
            if (map[testTile.myX, testTile.myY+1] != null)
            {
                tiles.Add(map[testTile.myX, testTile.myY+1]);   
            }
        }
        if(testTile.myY-1 > -1)
        {
            if (map[testTile.myX, testTile.myY-1] != null)
            {
                tiles.Add(map[testTile.myX, testTile.myY-1]);   
            }
        }

        return tiles[Random.Range(0, tiles.Count)];
        // return tiles.Count == 0 ? null : tiles[Random.Range(0, tiles.Count)];
    }
}

public enum CultureActionType
{
    Food,
    War,
    Religion,
    Labor,
    Happiness,
}