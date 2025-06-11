using System;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

public class PersonOfInterest
{
    public int age;
    public PersonOfInterest spouse = null;
    public PersonOfInterestRole role = PersonOfInterestRole.King;
    public Culture originalCulture;
    public string name;
    public int votingInfluence = 0;
    public List<CultureActionType> votingMatrix = new List<CultureActionType>();
    public PersonOfInterest(int age, PersonOfInterest spouse, PersonOfInterestRole role,Culture culture,string name)
    {
        this.age = age;
        this.spouse = spouse;
        this.role = GiveRole(role);
        this.originalCulture = culture;
        this.name = name;
        this.votingMatrix = MakeVotingMatrix();
    }

    private List<CultureActionType> MakeVotingMatrix()
    {
        // Get all enum values as a list
        var enumValues = Enum.GetValues(typeof(CultureActionType)).Cast<CultureActionType>().ToList();

        // Shuffle the list randomly
        var random = new System.Random();
        enumValues = enumValues.OrderBy(x => random.Next()).ToList();

        return enumValues; // Return the randomized list
    }

    public PersonOfInterestRole GiveRole(PersonOfInterestRole personOfInterestRole)
    {
        switch (role)
        {
            case PersonOfInterestRole.King:
                votingInfluence = Random.Range(60, 90);
                break;
            case PersonOfInterestRole.Queen:
                votingInfluence = Random.Range(60, 90);
                break;
            case PersonOfInterestRole.Royalty:
                votingInfluence = Random.Range(10, 20);
                break;
            case PersonOfInterestRole.Advisor:
                votingInfluence = Random.Range(20, 40);
                break;
            case PersonOfInterestRole.Diplomat:
                votingInfluence = Random.Range(10, 20);
                break;
            case PersonOfInterestRole.Religion:
                votingInfluence = Random.Range(40, 70);
                break;
            case PersonOfInterestRole.Labor:
                votingInfluence = Random.Range(40, 70);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(role), role, null);
        }
        return personOfInterestRole;
    }

    public List<CultureAction> MakeVoteFromActions(List<CultureAction> possibleActions)
    {
        // Prioritize and filter the possible actions based on the voting matrix
        var prioritizedActions = possibleActions
            .OrderBy(action => votingMatrix.IndexOf(action.type)) // Prioritize based on matrix position
            .ToList();

        // Calculate the number of votes to cast
        var voteNum = votingInfluence / 10;
        var votes = new List<CultureAction>();

        var weightedActions = new List<CultureAction>();
        for (int i = 0; i < prioritizedActions.Count; i++)
        {
            // Higher priority (lower index in prioritizedActions) gets more "weight"
            int weight = prioritizedActions.Count - i;

            // Add each action to the weighted list based on its weight
            for (int j = 0; j < weight; j++)
            {
                weightedActions.Add(prioritizedActions[i]);
            }
        }

        // Perform weighted random voting
        for (int i = 0; i < voteNum; i++)
        {
            votes.Add(weightedActions[Random.Range(0, weightedActions.Count)]);
        }

        return votes;
    }
}

public enum PersonOfInterestRole
{
    King,
    Queen,
    Royalty,
    Advisor,
    Diplomat,
    Religion,
    Labor,
}
