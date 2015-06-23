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
        private Faction oldFaction = null;

        public JobDriver_EatCorpse()
        {
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            //oldFaction = pawn.Faction;
            //pawn.SetFactionDirect(Faction.OfColony);

            this.FailOnDestroyed<JobDriver_EatCorpse>(TargetIndex.A);
            this.FailOn<JobDriver_EatCorpse>(eaterIsKilled);
            yield return Toils_Reserve.Reserve(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);

            Toil chewCorpse = new Toil
            {
                defaultCompleteMode = ToilCompleteMode.Instant,
            };
            chewCorpse.initAction = new Action(doChewCorpse);
            //chewCorpse.AddFinishAction(new Action(resetFaction));
            chewCorpse.WithEffect(EffecterDef.Named("EatMeat"), TargetIndex.A);
            chewCorpse.EndOnDespawned(TargetIndex.A);
            yield return chewCorpse;
        }

        private bool eaterIsKilled()
        {
            return pawn.Dead || pawn.Downed || pawn.HitPoints == 0;
        }

        private void doChewCorpse()
        {
            Corpse corpse = TargetThingA as Corpse;
            TameablePawn pet = pawn as TameablePawn;
            if (corpse != null)
            {
                /// changed to used butchering recipe directly and only place leftovers.
                //Pawn butcher = Find.ListerPawns.FreeColonists.First();
                //corpse.ButcherProducts(butcher, 1.0f).ToList<Thing>();
                Log.Message("Attempting to butch corpse");
                IntVec3 centerPos = corpse.Position;
                List<Thing> leftOvers = GenRecipe.MakeRecipeProducts(DefDatabase<RecipeDef>.GetNamed("ButcherCorpseFlesh"), pet, null, corpse).ToList<Thing>();
                Thing leftOver = null;
                for (int i = 0; i < leftOvers.Count; i++)
                {
                    if (!GenPlace.TryPlaceThing(leftOvers[i], centerPos, ThingPlaceMode.Near, out leftOver))
                    {
                        Log.Warning("Couldn't drop products");
                        pet.jobs.EndCurrentJob(JobCondition.Incompletable);
                    }
                    if (leftOver != null)
                        leftOver.SetForbidden(true);
                }
                if ((pet.IsColonyPet) && (pawn.needs.mood.thoughts != null))
                    pawn.needs.mood.thoughts.TryGainThought(ThoughtDef.Named("AteStraightFromCorpse"));
                corpse.Destroy();
            }
            pet.jobs.EndCurrentJob(JobCondition.Succeeded);
        }

        private void resetFaction()
        {
            if (oldFaction != null)
            {
                pawn.SetFactionDirect(oldFaction);
            }
            else
                pawn.SetFactionDirect(null);
        }
    }
}
