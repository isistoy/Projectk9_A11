using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using Verse.AI;

namespace ProjectK9.AI
{
    public static class FoodAIUtility_Pets
    {
        private static IEnumerable<Thing> ButcherCorpseProducts(this Corpse corpse)
        {
            if (corpse.def.butcherProducts == null)
                yield break;
            int i = 0;
            if (i < corpse.def.butcherProducts.Count)
            {
                ThingCount counter = corpse.def.butcherProducts[i];
                Thing result = ThingMaker.MakeThing(counter.thingDef, null);
                result.stackCount = counter.count;
                yield return result;
                i++;
            }
        }
    }
}

