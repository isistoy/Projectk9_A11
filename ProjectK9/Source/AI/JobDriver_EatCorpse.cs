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

            Toil chewCorpse = new Toil
            {
                defaultCompleteMode = ToilCompleteMode.Instant,
            };
            chewCorpse.initAction = new Action(finishChewing);
            chewCorpse.WithEffect(EffecterDef.Named("EatMeat"), TargetIndex.A);
            chewCorpse.EndOnDespawned(TargetIndex.A);
            yield return chewCorpse;
        }

        private bool eaterIsKilled()
        {
            return pawn.Dead || pawn.Downed || pawn.HitPoints == 0;
        }

        private void finishChewing()
        {
            Corpse corpse = TargetThingA as Corpse;

            if (corpse != null)
            {
                /// changed to used butchering recipe directly and only place leftovers.
                //Pawn butcher = Find.ListerPawns.FreeColonists.First();
                //corpse.ButcherProducts(butcher, 1.0f).ToList<Thing>();
                TameablePawn pet = pawn as TameablePawn;
                Log.Message("Attempting to butch corpse");
                IntVec3 centerPos = corpse.Position;
                List<Thing> leftovers =
                    GenRecipe.MakeRecipeProducts(DefDatabase<RecipeDef>.GetNamed("ButcherCorpseFlesh"), pet, null, corpse).ToList<Thing>();
                if (leftovers.Count != 0)
                {
                    for (int i = 0; i < leftovers.Count; i++)
                    {
                        Thing placedLeftover = null;
                        if (!GenPlace.TryPlaceThing(leftovers[i], centerPos, ThingPlaceMode.Near, out placedLeftover))
                        {
                            Log.Error("Couldn't drop products");
                            pet.jobs.EndCurrentJob(JobCondition.Incompletable);
                        }
                        else
                        {
                            placedLeftover.SetForbidden(true);
                        }
                    }
                    //if ((pet.IsColonyPet) && (pawn.needs.mood.thoughts != null))
                    //    pawn.needs.mood.thoughts.TryGainThought(ThoughtDef.Named("AteStraightFromCorpse"));
                }
                corpse.Destroy(DestroyMode.Vanish);
                pet.jobs.EndCurrentJob(JobCondition.Succeeded);
            }
        }
    }
}
