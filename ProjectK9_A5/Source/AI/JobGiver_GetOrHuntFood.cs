using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse.AI;
using Verse;
using RimWorld;

namespace ProjectK9.AI
{
    class JobGiver_GetOrHuntFood : ThinkNode_JobGiver
    {
        protected override Job TryGiveTerminalJob(Pawn pawn)
        {
            RegionTraverseParameters traverseParams = RegionTraverseParameters.For(pawn);
            ThingRequest request = ThingRequest.ForGroup(ThingRequestGroup.EdibleNotPlant);
            IEnumerable<Thing> things = Find.ListerThings.ThingsMatching(request).Where(food => 
                   ReservationUtility.CanReserveAndReach(pawn, food, ReservationType.Total, PathMode.Touch) 
                && food.def.food.isMeat);
            Thing thing = GenClosest.ClosestThingReachableGlobal(pawn.Position, things, PathMode.Touch, traverseParams);
            if (thing != null && isNearby(pawn, thing))
            {
                
                PawnPath path = PathFinder.FindPath(new PathRequest()
                {
                    start = pawn.Position,
                    dest = (TargetPack)thing,
                    pathParams = PathParameters.stupid,
                    pawn = pawn
                });

                if (path != PawnPath.NotFound)
                {
                    if (pawn.Faction == Find.FactionManager.FirstFactionOfDef(FactionDef.Named("ColonyPets"))) {
                        if (thing.def.food.isMeat)
                            pawn.psychology.thoughts.GainThought(new Thought(ThoughtDef.Named("AteMeat")));
                        if (thing.def.food.eatenDirectThought == ThoughtDef.Named("AteHumanoidMeatDirect"))
                            pawn.psychology.thoughts.GainThought(new Thought(ThoughtDef.Named("HumanMeatIsYummy")));
                    }
                        
                        

                    return new Job(JobDefOf.EatFood, (TargetPack) thing);
                }
            }

            // Find the closest dead meaty-thing and eat it.
            IEnumerable<Thing> corpses = Find.ListerThings.ThingsInGroup(ThingRequestGroup.Corpse)
                .Where(corpse => isNearby(pawn, corpse) && canReserve(pawn, corpse, ReservationType.Total));
            Corpse closestCorpse = GenClosest.ClosestThingReachableGlobal(pawn.Position, corpses, PathMode.Touch, traverseParams, 100f) as Corpse;
            if (closestCorpse != null)
            {
                Job eatCorpse = new Job(DefDatabase<JobDef>.GetNamed("EatCorpse"), closestCorpse);
                return eatCorpse;
            }

            // Find the closest animal smaller than yourself, and then hunt it
            IEnumerable<Pawn> possiblePrey = Find.ListerPawns.AllPawns.Where(prey => isPossiblePrey(prey, pawn));
            Pawn closestPrey = GenClosest.ClosestThingReachableGlobal(pawn.Position, possiblePrey, PathMode.Touch, traverseParams) as Pawn;
            if (closestPrey != null) {
                Job attackPrey = new Job(DefDatabase<JobDef>.GetNamed("Kill"), closestPrey);
                return attackPrey;
            }

            return null;
        }

        private bool isPossiblePrey(Pawn prey, Pawn hunter)
        {
            return hunter != prey 
                && !isFriendly(hunter, prey)
                && isNearby(hunter, prey)
                && canReserve(hunter, prey, ReservationType.Total)
                && (prey.RaceDef.bodySize < hunter.RaceDef.bodySize || hunter.food.Food.UrgentlyHungry);
        }

        private bool isFriendly(Pawn hunter, Pawn prey)
        {
            Faction petFaction = Find.FactionManager.FirstFactionOfDef(FactionDef.Named("ColonyPets"));

            if (hunter.Faction == petFaction)
                return (prey.Faction == Faction.OfColony || prey.Faction == petFaction) || prey.IsPrisonerOfColony;
            else
                return prey.def == hunter.def;

        }

        private bool isMeaty(Pawn pawn, Thing thing)
        {
            Corpse corpse = thing as Corpse;
            
            if (corpse == null)
                return false;
            
            return isNearby(pawn, corpse)
                && canReserve(pawn, corpse, ReservationType.Total)
                && corpse.sourcePawn.RaceDef.isFlesh;
        }

        private bool canReserve(Pawn pawn, Thing target, ReservationType reserveType)
        {
            return ReservationUtility.CanReserve(pawn, (TargetPack)target, reserveType);
        }

        private bool isNearby(Pawn pawn, Thing thing)
        {
            return (pawn.Position.ToVector3() - thing.Position.ToVector3()).sqrMagnitude <= 100f * 100f;
        }

    }
}
