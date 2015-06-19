using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;
using Verse.AI;
using RimWorld;

namespace ProjectK9.AI
{
    class JobGiver_GetRestForAnimal : ThinkNode_JobGiver
    {
        public override float GetPriority(Pawn pawn)
        {
            Need_Rest rest = pawn.needs.rest;
            if (rest != null)
            {
                if (rest.CurLevel < 0.55f)
                {
                    return 8f;
                }
            }
            return 0f;
        }
      
        protected override Job TryGiveTerminalJob(Pawn pawn)
        {
            if (pawn.Faction == Faction.OfColony)
            {
                Building_Bed bedFor = pawn.ownership.ownedBed;
                if (bedFor == null)
                {
                    bedFor = findUnownedBed(pawn);
                    if (bedFor != null && bedFor.owner != pawn)
                    {
                        // If it's owned by the HolderPawn, then we need to remove that, or else the game is going to try and
                        // make it unclaim the bed and then it'll error out.
                        if (bedFor.owner == PetBed.PetBedHolder)
                        {
                            bedFor.owner = null;
                        }
                        else
                            bedFor = null;
                    }
                }

                if (bedFor != null)
                {
                    Log.Message("bed found");
                    return new Job(DefDatabase<JobDef>.GetNamed("SleepForAnimals"), bedFor);
                }
            }
            Log.Message("No bed found");
            return new Job(DefDatabase<JobDef>.GetNamed("SleepForAnimals"), new TargetInfo(GenCellFinder.RandomStandableClosewalkCellNear(pawn.Position, 6)));
        }

        private Building_Bed findUnownedBed(Pawn pawn)
        {
            TraverseParms petBedParms = TraverseParms.For(pawn);
            ThingRequest petBedRequest = ThingRequest.ForDef(DefDatabase<ThingDef>.GetNamed("PetBed"));
            Predicate<Thing> petBedPredicate = t =>
            {
                PetBed bed = t as PetBed;
                if (!pawn.CanReserveAndReach(t, PathEndMode.OnCell, Danger.Some, 1))
                {
                    return false;
                }
                if (bed.owner == PetBed.PetBedHolder)
                {
                    return true;
                }
                return false;

            };
            return GenClosest.ClosestThingReachable(pawn.Position, petBedRequest, PathEndMode.InteractionCell, petBedParms, 9999f, petBedPredicate) as PetBed;
        }
    }
}
