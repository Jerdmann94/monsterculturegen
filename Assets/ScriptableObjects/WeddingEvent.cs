using System.Collections.Generic;
using UnityEngine;


    [CreateAssetMenu(fileName = "WeddingEvent", menuName = "Scriptable Objects/WeddingEvent")]
    public class WeddingEvent : ACulturalEvent
    {


        public override void DoAction(List<Culture> cultures)
        {
            foreach (var VARIABLE in cultures)
            {
                Debug.Log("in wedding people of interest count " + VARIABLE.peopleOfInterest.Count);
            }
            //Collect possible marriage target
            List<PersonOfInterest> marriageTargets = new List<PersonOfInterest>();
            var target = GetMarriageTarget(cultures[0]);
            if (target == null)
            {
                Debug.Log("No marriage target found");
                return;
            }
            GetSpouseFromFriends(cultures[0], marriageTargets,target);
            foreach (var culture in cultures[0].friends)
            {
                GetSpouseFromFriends(culture, marriageTargets, target);
            }

            if (marriageTargets.Count == 0)
            {
                Debug.Log("No marriage spouse found");
                return;
            }
            var newSpouse = marriageTargets[Random.Range(0, marriageTargets.Count)];
            target.spouse = newSpouse;
            newSpouse.spouse = target;
            if(newSpouse.originalCulture != target.originalCulture)
                cultures[0].friends.Add(newSpouse.originalCulture);
            Debug.Log(target.name + " from culture "+ cultures[0].name+" and " + newSpouse.name + " were married");
        }

        private PersonOfInterest GetMarriageTarget(Culture targetCulture)
        {
//            Debug.Log(targetCulture.peopleOfInterest.Count);
            foreach (var person in targetCulture.peopleOfInterest)
            {
                if(person.age >= targetCulture.birthingAge && person.spouse == null )
                {
                    return(person);
                }
            }

            return null;
        }

        private void GetSpouseFromFriends(Culture culture, List<PersonOfInterest> marriageTargets,PersonOfInterest target)
        {
            foreach (var person in culture.peopleOfInterest)
            {
                if(person.age > culture.birthingAge && person.spouse == null && person != target)
                {
                    marriageTargets.Add(person);
                }
            }
            
        }
    }

 

