using System.Collections.Generic;
using Harmony;
using TUNING;

namespace SuperAnimal
{
    public class SuperAnimal
    {
        public static void OnLoad()
        {
            CREATURES.CONVERSION_EFFICIENCY.BAD_1 += 1f;
            CREATURES.CONVERSION_EFFICIENCY.BAD_2 += 1f;
            CREATURES.CONVERSION_EFFICIENCY.NORMAL += 1f;
            CREATURES.CONVERSION_EFFICIENCY.GOOD_1 += 1f;
            CREATURES.CONVERSION_EFFICIENCY.GOOD_2 += 1f;
            CREATURES.CONVERSION_EFFICIENCY.GOOD_3 += 1f;
            var goodTraitsList = Traverse.Create<DUPLICANTSTATS>().Field<List<DUPLICANTSTATS.TraitVal>>("GOODTRAITS")
                .Value;
            var unwantedTraits = new List<string>
            {
                "InteriorDecorator",
                "Uncultured",
                "SimpleTastes",
                "Foodie"
            };
            goodTraitsList.RemoveAll(val => unwantedTraits.Contains(val.id));
        }
    }
}