using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using Verse.AI;
using System.Runtime.InteropServices;

namespace ProjectK9.AI
{
    public static class RestAIUtility_Animal
    {
        //private static JobDef sleepJobDef = DefDatabase<JobDef>.GetNamed("SleepForAnimals");
        //public static JobDef SleepJobDef
        //{
        //    get
        //    {
        //        return sleepJobDef;
        //    }
        //}

        private static JobDef rescueJobDef = DefDatabase<JobDef>.GetNamed("RescueForAnimals");
        public static JobDef RescueJobDef
        {
            get
            {
                return rescueJobDef;
            }
        }

        public static bool BedValidator(Pawn traveler, Pawn patient, Thing t, bool sleeperWillBePrisoner, bool checkSocialProperness)
        {
            if (!traveler.CanReserveAndReach(t, PathEndMode.OnCell, Danger.Some, 1))
            {
                return false;
            }
            Building_Bed bed = (Building_Bed)t;
            if (sleeperWillBePrisoner)
            {
                if (!bed.ForPrisoners)
                {
                    return false;
                }
                if (!bed.Position.IsInPrisonCell())
                {
                    return false;
                }
            }
            else
            {
                if (bed.Faction != traveler.Faction)
                {
                    return false;
                }
                if (bed.ForPrisoners)
                {
                    return false;
                }
            }
            if (bed.Medical)
            {
                if (!patient.health.CanUseMedicalBed)
                {
                    return false;
                }
                if ((bed.CurOccupant != null) && (bed.CurOccupant != patient))
                {
                    return false;
                }
            }
            else if ((bed.owner != null) && (bed.owner != patient))
            {
                return false;
            }
            if (checkSocialProperness && !bed.IsSociallyProper(patient, sleeperWillBePrisoner))
            {
                return false;
            }
            if (bed.IsForbidden(traveler))
            {
                return false;
            }
            if (((Thing)bed).IsBurning())
            {
                return false;
            }
            return true;
        }

        public static Thing FindBedForAnimal(Pawn traveler, TameablePawn sleeper, bool sleeperWillBePrisoner, bool checkSocialProperness, [Optional, DefaultParameterValue(false)] bool forceCheckMedBed)
        {
            //if (sleeper.health.CanUseMedicalBed)
            //{
            //    if ((sleeper.InBed() && sleeper.CurrentBed().Medical) && BedValidator(traveler, sleeper, sleeper.CurrentBed(), false, false))
            //    {
            //        return sleeper.CurrentBed();
            //    }
            //    for (int j = 0; j < bedDefsBestToWorst_Medical.Count; j++)
            //    {
            //        predicate = new Predicate<Thing>(storeyd.<>m__B5);
            //        Building_Bed bed = (Building_Bed) GenClosest.ClosestThingReachable(patient.Position, ThingRequest.ForDef(bedDefsBestToWorst_Medical[j]), PathEndMode.OnCell, TraverseParms.For(storeyd.traveler, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, predicate, null, -1, false);
            //        if (bed != null)
            //        {
            //            return bed;
            //        }
            //    }
            //}
            if (((sleeper.ownership != null) && (sleeper.ownership.ownedBed != null)) && BedValidator(traveler, sleeper, sleeper.ownership.ownedBed, false, false))
            {
                return sleeper.ownership.ownedBed;
            }
            return FindUnownedBed(sleeper);
            //for (int i = 0; i < bedDefsBestToWorst_RestEffectiveness.Count; i++)
            //{
            //    predicate = new Predicate<Thing>(storeyd.<>m__B6);
            //    Building_Bed bed2 = (Building_Bed) GenClosest.ClosestThingReachable(patient.Position, ThingRequest.ForDef(bedDefsBestToWorst_RestEffectiveness[i]), PathEndMode.OnCell, TraverseParms.For(storeyd.traveler, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, predicate, null, -1, false);
            //    if (bed2 != null)
            //    {
            //        if (patient.ownership != null)
            //        {
            //            patient.ownership.UnclaimBed();
            //        }
            //        return bed2;
            //    }
            //}
            //return null;
        }

        public static Building_Bed FindUnownedBed(Pawn pawn)
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
                    bed.owner = null;
                    return true;
                }
                return false;

            };
            return GenClosest.ClosestThingReachable(pawn.Position, petBedRequest, PathEndMode.InteractionCell, petBedParms, 9999f, petBedPredicate) as PetBed;
        }

        public static void Notify_TuckedIntoBedAnimal(Pawn animal, Building_Bed DropBed)
        {
            animal.Position = DropBed.Position;
            animal.Notify_Teleported();
            animal.jobs.StartJob(new Job(JobDefOf.LayDown, DropBed), JobCondition.InterruptForced, null, false, true);
        }
    }
}
