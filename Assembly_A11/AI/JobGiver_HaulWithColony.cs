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
            // Only haul with colonists, not with group or independantly
            TameablePawn pet = pawn as TameablePawn;
            if (pet == null)
                return null;
            
            // We can search for some close haulable element for our pet
            Boolean isHauler = !pet.story.DisabledWorkTypes.Contains(WorkTypeDefOf.Hauling);
            IEnumerable<Pawn> colonists = Find.ListerPawns.FreeColonists;
            if (isHauler)
            {
                Log.Message("Starting looking for Haul job");
                bool hasHauler=false;
                foreach (Pawn colonist in colonists)
                {
                    //if (colonist.jobs.curJob != null && colonist.jobs.curJob.def == JobDefOf.HaulToContainer)
                    if (colonist.jobs.curJob != null && ((colonist.jobs.curJob.def == JobDefOf.HaulToContainer)
                        || (colonist.jobs.curJob.def == JobDefOf.HaulToCell)))
                    {
                        hasHauler = true;
                        Log.Message("Some colonists hauling");
                        ThingRequest haulThingReq = ThingRequest.ForGroup(ThingRequestGroup.HaulableAlways);
                        Predicate<Thing> haulThingPredicate = t =>
                        {  
                            if (!t.IsForbidden(pet.Faction) && !t.IsForbidden(Faction.OfColony))
                                return true;
                            return false;
                        };
                        IEnumerable<Thing> globalSearchSet = ListerHaulables.ThingsPotentiallyNeedingHauling();
                        Thing closestHaulableThing = GenClosest.ClosestThingReachable(colonist.jobs.curJob.targetA.Cell, haulThingReq,
                            PathEndMode.OnCell, TraverseParms.For(pet), 60f, haulThingPredicate, globalSearchSet);

                        if (closestHaulableThing != null)
                        {
                            Log.Message("Reserving thing for job");
                            if (HaulAIUtility.PawnCanAutomaticallyHaulFast(pet, closestHaulableThing))
                            {
                                Log.Message("starting haul job");

                                // TBD
                                Job job = HaulAIUtility_Pets.HaulToStorageJob(pet, closestHaulableThing);
                                if (job != null)
                                {
                                    return job;
                                }
                                else
                                {
                                    Log.Message("Impossible to get the Job from AI");
                                    return null;
                                }
                            }
                        }
                        else
                            Log.Message("no haulable thing found");
                    }
                }
                if (!hasHauler)
                    Log.Message("No colonist hauling");
            }
            Log.Message("No hauling Job");
            return null;
        }
    }
}
