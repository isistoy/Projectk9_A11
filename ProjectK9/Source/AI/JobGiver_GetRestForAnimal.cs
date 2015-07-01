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

            if ((pawn.needs.rest.CurLevel < 0.50f) && !pawn.needs.food.Starving)
            {
                return 8f;
            }
            return 0f;
        }

        protected override Job TryGiveTerminalJob(Pawn pawn)
        {
            TameablePawn tameable = pawn as TameablePawn;
            if ((tameable != null) && ((tameable.jobs.curJob == null) || tameable.Awake()))
            {
                if ((!pawn.Downed) && (!pawn.Broken))
                {
                    //Log.Message(string.Concat(pawn, " trying to sleep"));
                    TameablePawn pet = pawn as TameablePawn;
                    if ((pet != null) && pet.IsColonyPet)
                    {
                        Building_Bed bedFor = pawn.ownership.ownedBed;

                        if (bedFor == null)
                            bedFor = findUnownedBed(pawn);

                        if ((bedFor != null) && (bedFor.owner != pawn))
                        {
                            // If it's owned by the HolderPawn, then we need to remove that, or else the game is going to try and
                            // make it unclaim the bed and then it'll error out.
                            if (bedFor.owner == PetBed.PetBedHolder)
                            {
                                bedFor.owner = null;
                            }
                        }
                        if (bedFor != null)
                            return new Job(RestAIUtility_Animal.GetSleepJobDef(), bedFor);
                    }
                    return new Job(RestAIUtility_Animal.GetSleepJobDef(), GenCellFinder.RandomStandableClosewalkCellNear(pawn.Position, 4));
                    //return new Job(JobDefOf.LayDown, GenCellFinder.RandomStandableClosewalkCellNear(pawn.Position, 4));
                }
            }
            return null;
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
