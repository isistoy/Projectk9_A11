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
        public override float GetPriority(Pawn pawn)
        {
            Need_Food food = pawn.needs.food;
            if ((food != null) && (pawn.needs.food.CurCategory >= HungerCategory.UrgentlyHungry))
            {
                return 8.5f;
            }
            return 0f;
        }

        protected override Job TryGiveTerminalJob(Pawn pawn)
        {
            TameablePawn pet = (TameablePawn)pawn;
            TraverseParms traverseParams = TraverseParms.For(pawn);
            Log.Message("Checking food");
            // Find the closest meaty-thing and eat it.
            ThingRequest meatRequest = ThingRequest.ForGroup(ThingRequestGroup.FoodNotPlant);
            Predicate<Thing> availMeatPredicate = food =>
            {
                return isMeaty(pawn, food);
            };

            //IEnumerable<Thing> things = Find.ListerThings.ThingsMatching(request).Where(food => 
            //       ReservationUtility.CanReserveAndReach(pet, food, PathEndMode.Touch,pet.NormalMaxDanger()) 
            //    && food.def.ingestible.IsMeat);

            Thing thing = GenClosest.ClosestThingReachable(pawn.Position, meatRequest, PathEndMode.Touch, traverseParams, 100f, availMeatPredicate);
            if (thing != null)
            {
                Log.Message(string.Concat(pet, " Found meat ", thing));
                PawnPath path = PathFinder.FindPath(pawn.Position, thing, traverseParams);

                if (path != PawnPath.NotFound)
                {
                    if (pet.IsColonyPet && thing.SelectableNow() && pawn.needs.mood.thoughts != null)
                    {
                        Log.Message("Meat and colonist. Ingesting...");
                        if (thing.def.ingestible.IsMeat)
                            pawn.needs.mood.thoughts.TryGainThought(ThoughtDef.Named("AteMeat"));
                        if (thing.def.ingestible.ingestedDirectThought == ThoughtDef.Named("AteHumanoidMeatDirect"))
                            pawn.needs.mood.thoughts.TryGainThought(ThoughtDef.Named("HumanMeatIsYummy"));
                        return new Job(JobDefOf.Ingest, thing);
                    }
                    return new Job(JobDefOf.Ingest, thing);
                }
            }

            // Find the closest dead meaty-thing and eat it.
            //IEnumerable<Thing> corpses = Find.ListerThings.ThingsInGroup(ThingRequestGroup.Corpse)
            //    .Where(corpse => isNearby(pet, corpse) && canReserve(pet, corpse, ReservationType.Total));
            ThingRequest corpseRequest = ThingRequest.ForGroup(ThingRequestGroup.Corpse);
            Predicate<Thing> availCorpsePredicate = corpse =>
            {
                if (isNearby(pawn, corpse)
                    && ReservationUtility.CanReserve(pawn, corpse))
                {
                    return true;
                }
                return false;
            };

            Corpse closestCorpse = GenClosest.ClosestThingReachable(pawn.Position, corpseRequest, PathEndMode.Touch, traverseParams, 100f, availCorpsePredicate) as Corpse;
            if (closestCorpse != null)
            {
                Log.Message(string.Concat(pet, " Found corpse ", closestCorpse));
                return new Job(DefDatabase<JobDef>.GetNamed("EatCorpse"), closestCorpse);
            }

            // Find the closest animal smaller than yourself, and then hunt it
            if (pawn.jobs.curJob == null)
            {
                ThingRequest preyRequest = ThingRequest.ForGroup(ThingRequestGroup.Pawn);
                Predicate<Thing> availPreyPredicate = p =>
                {
                    Pawn prey = p as Pawn;
                    return isPossiblePrey(prey, pawn);
                };

                //IEnumerable<Pawn> possiblePrey = Find.ListerPawns.AllPawns.Where(prey => isPossiblePrey(prey, pet));
                Pawn closestPrey = GenClosest.ClosestThingReachable(pawn.Position, preyRequest, PathEndMode.Touch, traverseParams, 100f, availPreyPredicate) as Pawn;
                if (closestPrey != null)
                {
                    Log.Message(string.Concat(pet, " Found prey ", closestPrey));
                    return new Job(JobDefOf.AttackMelee, closestPrey) { killIncappedTarget = true };
                    //DefDatabase<JobDef>.GetNamed("Kill")
                }
            }

            return null;
        }

        private bool isPossiblePrey(Pawn prey, Pawn hunter)
        {
            Log.Message("isPossiblePrey - CanReserveAndReach");
            return (hunter != prey)
                && !prey.Dead
                && !isFriendly(hunter, prey)
                && isNearby(hunter, prey)
                //&& PathFinder.FindPath(hunter.Position, (TargetInfo)prey, hunter, PathEndMode.Touch) != null
                && ReservationUtility.CanReserveAndReach(hunter, prey, PathEndMode.Touch, Danger.Some)
                && (prey.RaceProps.bodySize < hunter.RaceProps.bodySize || (hunter.needs.food.CurCategory == HungerCategory.UrgentlyHungry));
        }

        private bool isFriendly(Pawn hunter, Pawn prey)
        {
            TameablePawn pet = (TameablePawn)hunter;
            if (pet.IsColonyPet)
            {
                TameablePawn preyT = prey as TameablePawn;
                if (preyT == null)
                {
                    return (prey.Faction == Faction.OfColony) || (prey.def == hunter.def) || prey.IsPrisonerOfColony;
                }
                else
                    return (prey.Faction == Faction.OfColony) || preyT.IsColonyPet || prey.IsPrisonerOfColony;
            }
            else
                return prey.def == hunter.def;

        }

        private bool isMeaty(Pawn pawn, Thing thing)
        {
            Corpse corpse = thing as Corpse;

            Log.Message("isMeaty - CanReserveAndReach");
            if (corpse == null)
            {
                return isNearby(pawn, thing)
                    && ReservationUtility.CanReserveAndReach(pawn, thing, PathEndMode.OnCell, Danger.Some)
                    && thing.def.ingestible.IsMeat;
            }
            else
            {
                return isNearby(pawn, corpse)
                && ReservationUtility.CanReserveAndReach(pawn, corpse, PathEndMode.OnCell, Danger.Some)
                && corpse.innerPawn.RaceProps.isFlesh;
            }

        }

        private bool isNearby(Pawn pawn, Thing thing)
        {
            return (pawn.Position.ToVector3() - thing.Position.ToVector3()).sqrMagnitude <= 100f * 100f;
        }

    }
}
