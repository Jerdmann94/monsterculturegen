using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BirthEvent", menuName = "Scriptable Objects/BirthEvent")]
public class BirthEvent : ACulturalEvent
{
    public override void DoAction(List<Culture> cultures)
    {
        var culture = cultures[0];
        var poi = culture.peopleOfInterest;
        var child = new PersonOfInterest(0, null, PersonOfInterestRole.Royalty, null,
            null);
        var possibleBirthers = new List<PersonOfInterest>();
        foreach (var person in poi)
        {
            if (person.age <= culture.birthingAge || person.spouse == null) continue;
            possibleBirthers.Add(person);
        }
//        Debug.Log("possible birther count " + possibleBirthers.Count);
        if (possibleBirthers.Count == 0)
        {
            Debug.Log("No birthing found");
            return;
        }
        
        foreach (var person in possibleBirthers)
        {
            var roles = new HashSet<PersonOfInterestRole> { person.role, person.spouse.role };
            var role = PersonOfInterestRole.King;
            switch (true)
            {
                case var _ when roles.Contains(PersonOfInterestRole.King):
                    role = PersonOfInterestRole.Royalty;
                    Debug.Log("At least one role is King");
                    break;

                case var _ when roles.Contains(PersonOfInterestRole.Queen):
                    role = PersonOfInterestRole.Royalty;
                    Debug.Log("At least one role is Queen");
                    break;

                case var _ when roles.Contains(PersonOfInterestRole.Advisor):
                    role = PersonOfInterestRole.Advisor;
                    Debug.Log("At least one role is Advisor");
                    break;
                
                case var _ when roles.Contains(PersonOfInterestRole.Diplomat):
                    role = PersonOfInterestRole.Diplomat;
                    Debug.Log("At least one role is Diplomat");
                    break;
                
                case var _ when roles.Contains(PersonOfInterestRole.Royalty):
                    role = PersonOfInterestRole.Royalty;
                    Debug.Log("At least one role is Royalty");
                    break;


                default:
                    Debug.Log("No significant roles found");
                    break;
            }
            child = new PersonOfInterest(0, null, role, culture,
                culture.name + " child" + 0);
            Debug.Log("A child was born to " + person.name + " and " + person.spouse.name + " of culture" + culture.name);
            break;
        }
        culture.peopleOfInterest.Add(child);
    }

    
}
