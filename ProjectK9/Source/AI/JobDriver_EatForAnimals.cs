using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

using RimWorld;
using Verse;
using Verse.AI;

namespace ProjectK9.AI
{

    public class JobDriver_EatForAnimals : JobDriver
    {
        private const TargetIndex DispenserInd = TargetIndex.A;
        private const TargetIndex FoodInd = TargetIndex.A;


        protected override IEnumerable<Toil> MakeNewToils()
        {
            if (TargetThingA is Building_NutrientPasteDispenser)
            {
                yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell).FailOnDespawnedOrForbidden<Toil>(TargetIndex.A);
                yield return Toils_Ingest.TakeMealFromDispenser(TargetIndex.A, pawn);
                yield return Toils_Ingest.CarryIngestibleToChewSpot().FailOnDestroyedOrForbidden<Toil>(TargetIndex.A);
                yield return Toils_Ingest.PlaceItemForIngestion(TargetIndex.A);
                if (pawn.Faction != null)
                {
                    yield return Toils_Reserve.Reserve(TargetIndex.A, 1);

                }
                yield return Toils_Ingest.ChewIngestible(pawn, 1f / pawn.GetStatValue(StatDefOf.EatingSpeed, true)).FailOnDespawned<Toil>(TargetIndex.A);
                yield return FoodAIUtility_Animals.FinalizeEatForAnimals(pawn, TargetIndex.A);
            }
            else
            {
                Toil resFood = new Toil();
                resFood.initAction = new Action(() =>
                    {
                        Pawn actor = resFood.actor;
                        Thing target = resFood.actor.CurJob.GetTarget(TargetIndex.A).Thing;
                        if ((FoodUtility.WillEatStackCountOf(actor, target.def) >= target.stackCount) && (!target.SpawnedInWorld || !Find.Reservations.TryReserve(actor, target, 1)))
                        {
                            actor.jobs.EndCurrentJob(JobCondition.Incompletable);
                        }
                    });
                resFood.defaultCompleteMode = ToilCompleteMode.Instant;
                yield return resFood;
                yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
                if (pawn.Faction != null)
                {
                    yield return Toils_Reserve.Reserve(TargetIndex.A, 1);
                }
                //else
                //{
                //    yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnDespawnedOrForbidden<Toil>(TargetIndex.A);
                //    yield return Toils_Ingest.PickupIngestible(TargetIndex.A, pawn);
                //    yield return Toils_Ingest.CarryIngestibleToChewSpot().FailOnDestroyedOrForbidden<Toil>(TargetIndex.A);
                //    yield return Toils_Ingest.PlaceItemForIngestion(TargetIndex.A);
                //    if (pawn.Faction != null)
                //    {
                //        yield return Toils_Reserve.Reserve(TargetIndex.A, 1);
                //    }                    
                //}
                yield return Toils_Ingest.ChewIngestible(pawn, 1f / pawn.GetStatValue(StatDefOf.EatingSpeed, true)).FailOnDespawned<Toil>(TargetIndex.A);
                yield return FoodAIUtility_Animals.FinalizeEatForAnimals(pawn, TargetIndex.A);
            }
        }
    }
}