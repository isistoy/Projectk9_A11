using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;

using Verse;
using Verse.AI;
using RimWorld;

namespace ProjectK9.AI
{
    public class JobDriver_EatCorpse : JobDriver
    {
        public JobDriver_EatCorpse()
        {
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyed(TargetIndex.A);
            this.FailOn(eaterIsKilled); 
            yield return Toils_Reserve.Reserve(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);

            // TODO: Loop this sound
            yield return Toils_Effects.MakeSound("EatStandard");

            Toil chewCorpse = new Toil
            {
                defaultCompleteMode = ToilCompleteMode.Instant,
            };
            chewCorpse.WithEffect(EffecterDef.Named("EatMeat"), TargetIndex.A);
            chewCorpse.EndOnDespawned(TargetIndex.A);
            chewCorpse.AddFinishAction(finishEating);
            yield return chewCorpse;
        }

        private bool eaterIsKilled()
        {
            return pawn.Dead || pawn.Downed || pawn.HitPoints == 0;
        }

        private void finishEating()
        {
            Corpse corpse = TargetThingA as Corpse;
            if (corpse != null) {
                
                Faction petFaction = Find.FactionManager.FirstFactionOfDef(FactionDef.Named("ColonyPets"));

                // TODO: Figure out a way to butcher a corpse without any Butcher skill
                Pawn butcher = Find.ListerPawns.FreeColonists.First();
                IEnumerable<Thing> products = corpse.ButcherProducts(butcher, 1.0f);
                List<Thing> leftovers = new List<Thing>();
                foreach (Thing meatPile in products)
                {
                    //while (meatPile.stackCount > 0 && pawn.needs.food.CurInstantLevel >= 0.95f)
                    //{
                    //    Thing meat = meatPile.SplitOff(1);
                    //    pawn.needs.food.FinishEating(meat);
                    //}
                    meatPile.Ingested(pawn, pawn.needs.food.NutritionWanted);
                    if ((meatPile != null) && (meatPile.stackCount > 0))
                        leftovers.Add(meatPile);
                }

                foreach (Thing leftover in leftovers)
                {
                    if (pawn.Faction != petFaction)
                        leftover.SetForbidden(true);
                    GenPlace.TryPlaceThing(leftover, corpse.Position, ThingPlaceMode.Near);
                }

                if (pawn.Faction == petFaction) 
                    pawn.needs.mood.thoughts.TryGainThought(ThoughtDef.Named("AteStraightFromCorpse"));

                corpse.Destroy();
            }
        }

    }
}
