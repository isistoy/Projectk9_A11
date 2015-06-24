using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Verse;
using Verse.AI;
using RimWorld;

namespace ProjectK9.AI
{
    public class JobDriver_Tame : JobDriver
    {
        public JobDriver_Tame()
        {
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyed(TargetIndex.A);
            this.FailOnThingMissingDesignation(TargetIndex.A, DefDatabase<DesignationDef>.GetNamed("Tame"));
            this.FailOnDespawned(TargetIndex.A);
            yield return Toils_Reserve.Reserve(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);

            // TODO Create "Mesmerize" Toil to stop dog from walking around and acting while being tamed.

            Toil tame = new Toil
            {
                defaultCompleteMode = ToilCompleteMode.Delay,
                defaultDuration = 20f.SecondsToTicks()
            };
            tame.AddFinishAction(tamePawn);
            yield return tame;
        }

        private void tamePawn()
        {
            TameablePawn tamee = TargetThingA as TameablePawn;

            if (tamee == null)
                return;

            Log.Message("Initializing new Pet");
            TamePawnUtility.InitializeColonyPet(tamee);
            //Log.Message("Previously giving the pawn it's Keys");
            //tamee.inventory.container.TryAdd(ThingMaker.MakeThing(ThingDefOf.DoorKey));
            Designation tameDes = Find.DesignationManager.DesignationOn(TargetThingA, DefDatabase<DesignationDef>.GetNamed("Tame"));
            if (tameDes != null)
                Find.DesignationManager.RemoveDesignation(tameDes);
            Log.Message("Removed Taming designation");
        }
    }
}
