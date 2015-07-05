using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace ProjectK9
{
    public class StockGenerator_SlavePets : StockGenerator
    {
        public override IEnumerable<Thing> GenerateThings()
        {
            //if (Rand.Value >= Find.Storyteller.intenderPopulation.PopulationIntent)
            //{
            //    yield break;
            //}
            for (int i = 0; i < this.countRange.RandomInRange; i++ )
            {
                PawnKindDef kindDef = DefDatabase<PawnKindDef>.AllDefs.Where<PawnKindDef>( x =>
                {
                    return (x.race.thingClass == typeof(TameablePawn)); 
                }).RandomElement();
                Log.Message(string.Concat("choosen pawnkind is ", kindDef.defName));
                yield return TameablePawnGenerator.GenerateTameable(kindDef, Faction.OfColony);
            }
        }

        public override bool HandlesThingDef(ThingDef thingDef)
        {
            return ((thingDef.category == ThingCategory.Pawn) && thingDef.thingClass == typeof(TameablePawn));
        }
    }
}
