using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse.AI;
using Verse;
using RimWorld;

namespace ProjectK9.AI
{
    public class JobGiver_GetOrHuntFood : ThinkNode_JobGiver
    {
        public const int FOOD_DISTANCE = 50 * 50;
        
        public override float GetPriority(Pawn pawn)
        {
            Need_Food food = pawn.needs.food;
            if (food != null)
            {
                if (food.CurCategory >= HungerCategory.UrgentlyHungry)
                    return 7.5f;
            }
            return 0f;
        }

        protected override Job TryGiveTerminalJob(Pawn pawn)
        {
            TameablePawn pet = (TameablePawn)pawn;
            TraverseParms traverseParams = TraverseParms.For(pawn);
            
            // Find the closest meaty-thing and eat it.
            JobDef meatJobDef = FoodAIUtility_Animals.GetEatMeatJobDef();
            if ((pawn.jobs.curJob == null) || ((pawn.jobs.curJob.def != meatJobDef) && pawn.jobs.curJob.checkOverrideOnExpire))
            {
                ThingRequest meatRequest = ThingRequest.ForGroup(ThingRequestGroup.FoodNotPlant);
                Predicate<Thing> availMeatPredicate = food =>
                {
                    return (isMeaty(pawn, food) && (pet.IsColonyPet ? Find.Reservations.CanReserve(pet, food, 1) : true));
                };

                Thing thing = GenClosest.ClosestThingReachable(pawn.Position, meatRequest, PathEndMode.Touch, traverseParams, 100f, availMeatPredicate);
                if (thing != null)
                {
                    Log.Message(string.Concat(pet, " Found meat"));
                    if (thing.SelectableNow() && pawn.needs.mood != null)
                    {
                        if (thing.def.ingestible.IsMeat)
                            pawn.needs.mood.thoughts.TryGainThought(ThoughtDef.Named("AteMeat"));
                        if (thing.def.ingestible.ingestedDirectThought == ThoughtDefOf.AteHumanlikeMeatDirect)
                            pawn.needs.mood.thoughts.TryGainThought(ThoughtDef.Named("HumanMeatIsYummy"));
                    }
                    return new Job(meatJobDef, thing);
                }
            }

            // Find the closest dead meaty-thing and eat it.
            JobDef corpseJobDef = FoodAIUtility_Animals.GetEatCorpseJobDef();
            if ((pawn.jobs.curJob == null) || ((pawn.jobs.curJob.def != corpseJobDef) && pawn.jobs.curJob.checkOverrideOnExpire))
            {
                ThingRequest corpseRequest = ThingRequest.ForGroup(ThingRequestGroup.Corpse);
                Predicate<Thing> availCorpsePredicate = corpse =>
                {
                    return (isMeaty(pawn, corpse) && (pet.IsColonyPet ? Find.Reservations.CanReserve(pet, corpse, 1) : true));
                };

                Corpse closestCorpse = GenClosest.ClosestThingReachable(pawn.Position, corpseRequest, PathEndMode.Touch, traverseParams, 100f, availCorpsePredicate) as Corpse;
                if (closestCorpse != null)
                {
                    Log.Message(string.Concat(pet, " Found corpse"));
                    return new Job(corpseJobDef, closestCorpse);
                }
            }

            // Find the closest prey to hunt for food
            JobDef huntJobDef = FoodAIUtility_Animals.GetHuntForAnimalsJobDef();
            if ((pawn.jobs.curJob == null) || ((pawn.jobs.curJob.def != huntJobDef) && pawn.jobs.curJob.checkOverrideOnExpire))
            {
                ThingRequest preyRequest = ThingRequest.ForGroup(ThingRequestGroup.Pawn);
                Predicate<Thing> availPreyPredicate = p =>
                {
                    Pawn prey = p as Pawn;
                    return isPossiblePrey(prey, pawn);
                };

                Pawn closestPrey = GenClosest.ClosestThingReachable(pawn.Position, preyRequest, PathEndMode.Touch, traverseParams, 100f, availPreyPredicate) as Pawn;
                if (closestPrey != null)
                {
                    
                    Log.Message(string.Concat(pet, " Found prey"));
                    return new Job(huntJobDef)
                    {
                        targetA = closestPrey,
                        maxNumMeleeAttacks = 4,
                        checkOverrideOnExpire = true,
                        killIncappedTarget = true,
                        expiryInterval = 500
                    };
                }
            }

            return null;
        }

        private bool isPossiblePrey(Pawn prey, Pawn hunter)
        {
            return (hunter != prey)
                && !prey.Dead
                && !isFriendly(hunter, prey)
                && isNearby(hunter, prey)
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
            if (corpse == null)
            {
                return isNearby(pawn, thing)
                    && thing.def.ingestible.IsMeat;
            }
            else
            {
                return isNearby(pawn, corpse)
                && corpse.innerPawn.RaceProps.isFlesh;
            }

        }

        private bool isNearby(Pawn pawn, Thing thing)
        {
            return (pawn.Position - thing.Position).LengthHorizontalSquared <= FOOD_DISTANCE;
        }
    }
}
