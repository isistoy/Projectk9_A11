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
            if ((food != null) && (food.CurCategory == HungerCategory.UrgentlyHungry))
            {
                return 9.5f;
            }
            return 0f;
        }

        protected override Job TryGiveTerminalJob(Pawn pawn)
        {
            TameablePawn pet = pawn as TameablePawn;
            if (pet == null)
            {
                Log.Error("GetOrHuntFood not on Tameable");
                return null;
            }
            TraverseParms traverseParams = TraverseParms.For(pet);
            Log.Message("Checking food");
            // Find the closest meaty-thing and eat it.
            ThingRequest meatRequest = ThingRequest.ForGroup(ThingRequestGroup.FoodNotPlant);
            Predicate<Thing> availMeatPredicate = food =>
            {
                return isMeaty(pet, food);
            };

            //IEnumerable<Thing> things = Find.ListerThings.ThingsMatching(request).Where(food => 
            //       ReservationUtility.CanReserveAndReach(pet, food, PathEndMode.Touch,pet.NormalMaxDanger()) 
            //    && food.def.ingestible.IsMeat);

            Thing thing = GenClosest.ClosestThingReachable(pet.Position, meatRequest, PathEndMode.Touch, traverseParams, 100f, availMeatPredicate);
            if (thing != null)
            {
                Log.Message("Found meat");
                PawnPath path = PathFinder.FindPath(pet.Position, thing, traverseParams);

                if (path != PawnPath.NotFound)
                {
                    if (pet.IsColonyPet && thing.SelectableNow())
                    {
                        Log.Message("Meat and colonist. Ingesting...");
                        if (thing.def.ingestible.IsMeat)
                            pet.needs.mood.thoughts.TryGainThought(ThoughtDef.Named("AteMeat"));
                        if (thing.def.ingestible.ingestedDirectThought == ThoughtDef.Named("AteHumanoidMeatDirect"))
                            pet.needs.mood.thoughts.TryGainThought(ThoughtDef.Named("HumanMeatIsYummy"));
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
                if (isNearby(pet, corpse)
                    && ReservationUtility.CanReserve(pet, corpse))
                {
                    return true;
                }
                return false;
            };

            Corpse closestCorpse = GenClosest.ClosestThingReachable(pet.Position, corpseRequest, PathEndMode.Touch, traverseParams, 100f, availCorpsePredicate) as Corpse;
            if (closestCorpse != null)
            {
                Log.Message("Found corpse");
                return new Job(DefDatabase<JobDef>.GetNamed("EatCorpse"), closestCorpse);
            }

            // Find the closest animal smaller than yourself, and then hunt it
            ThingRequest preyRequest = ThingRequest.ForGroup(ThingRequestGroup.Pawn);
            Predicate<Thing> availPreyPredicate = p =>
            {
                Pawn prey = p as Pawn;
                return isPossiblePrey(prey, pet);
            };

            //IEnumerable<Pawn> possiblePrey = Find.ListerPawns.AllPawns.Where(prey => isPossiblePrey(prey, pet));
            Pawn closestPrey = GenClosest.ClosestThingReachable(pet.Position, preyRequest, PathEndMode.Touch, traverseParams, 100f, availPreyPredicate) as Pawn;
            if (closestPrey != null)
            {
                Log.Message("Found prey");
                return new Job(DefDatabase<JobDef>.GetNamed("Kill"), closestPrey);
            }

            return null;
        }

        private bool isPossiblePrey(Pawn prey, TameablePawn hunter)
        {
            return hunter != prey
                && !isFriendly(hunter, prey)
                && isNearby(hunter, prey)
                && ReservationUtility.CanReserveAndReach(hunter, prey, PathEndMode.OnCell, Danger.Some)
                && (prey.RaceProps.bodySize < hunter.RaceProps.bodySize || (hunter.needs.food.CurCategory == HungerCategory.UrgentlyHungry));
        }

        private bool isFriendly(TameablePawn hunter, Pawn prey)
        {
            Faction petFaction = Find.FactionManager.FirstFactionOfDef(FactionDef.Named("ColonyPets"));
            if (hunter.IsColonyPet)
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

            Log.Message("CanReserve");
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
