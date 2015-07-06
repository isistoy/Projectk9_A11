using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;
using Verse.AI;
using RimWorld;

namespace ProjectK9.AI
{
    public class JobGiver_HaultWithColony : ThinkNode_JobGiver
    {

        protected override Job TryGiveTerminalJob(Pawn pawn)
        {
            if ((pawn.jobs.curJob == null) || ((pawn.jobs.curJob.def != JobDefOf.HaulToCell)) && pawn.jobs.curJob.checkOverrideOnExpire)
            {
                // Only haul with colonists, not with group or independantly
                TameablePawn pet = pawn as TameablePawn;
                if (pet == null)
                    return null;

                // We can search for some close haulable element for our pet
                Boolean isHauler = !pet.story.DisabledWorkTypes.Contains(WorkTypeDefOf.Hauling);
                IEnumerable<Pawn> colonists = Find.ListerPawns.FreeColonists;
                if (isHauler)
                {
                    foreach (Pawn colonist in colonists)
                    {
                        if (colonist.jobs.curJob != null && ((colonist.jobs.curJob.def == JobDefOf.HaulToContainer)
                            || (colonist.jobs.curJob.def == JobDefOf.HaulToCell)))
                        {
                            //Log.Message(string.Concat(pawn, " found that ", colonist, " is hauling"));
                            ThingRequest haulThingReq = ThingRequest.ForGroup(ThingRequestGroup.HaulableAlways);
                            Predicate<Thing> haulThingPredicate = t =>
                            {
                                if (!t.IsForbidden(pet.Faction) 
                                    && !t.IsForbidden(Faction.OfColony) 
                                    && ReservationUtility.CanReserveAndReach(pawn, t, PathEndMode.OnCell, Danger.Some))
                                    return true;
                                return false;
                            };
                            IEnumerable<Thing> globalSearchSet = ListerHaulables.ThingsPotentiallyNeedingHauling();
                            Thing closestHaulableThing = GenClosest.ClosestThingReachable(colonist.jobs.curJob.targetA.Cell, haulThingReq,
                                PathEndMode.OnCell, TraverseParms.For(pet), 60f, haulThingPredicate, globalSearchSet);

                            if (closestHaulableThing != null)
                            {
                                if (HaulAIUtility.PawnCanAutomaticallyHaulFast(pet, closestHaulableThing))
                                {
                                    // TBD
                                    Job job = HaulAIUtility.HaulToStorageJob(pet, closestHaulableThing);
                                    if (job != null)
                                    {
                                        //Log.Message(string.Concat(pawn, " is hauling ", closestHaulableThing));
                                        return job;
                                    }
                                }
                            }
                        }
                    }
                } 
            }
            return null;
        }
    }
}
