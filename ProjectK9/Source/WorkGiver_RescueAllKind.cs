using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using Verse.AI;
using ProjectK9.AI;

namespace ProjectK9
{
    public class WorkGiver_RescueAllKind : WorkGiver_TakeToBed
    {
        // Fields
        private const float MinDistFromHostile = 40f;

        // Methods
        public WorkGiver_RescueAllKind()
        {
        }
        
        public override bool HasJobOnThing(Pawn pawn, Thing t)
        {
            Pawn p = t as Pawn;
            if (((p == null) || !p.Downed) || (((p.Faction != pawn.Faction) || p.InBed()) || !pawn.CanReserve(p, 1)))
            {
                return false;
            }
            IEnumerator<Pawn> enumerator = Find.ListerPawns.PawnsHostileToColony.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    Pawn current = enumerator.Current;
                    if (!current.Downed)
                    {
                        IntVec3 vec = current.Position - p.Position;
                        if (vec.LengthHorizontalSquared < 1600f)
                        {
                            return false;
                        }
                    }
                }
            }
            finally
            {
                if (enumerator == null)
                {
                }
                enumerator.Dispose();
            }

            Thing target = null;
            if (p is TameablePawn)
            {
                target = RestAIUtility_Animal.FindBedForAnimal(pawn, p as TameablePawn, false, false);
            }
            else
            {
                target = FindBed(pawn, p);
            }
            return ((target != null) && p.CanReserve(target, 1));

        }
        public override Job JobOnThing(Pawn pawn, Thing t)
        {
            Pawn targetA = t as Pawn;
            if (targetA is TameablePawn)
            {
                Log.Message(string.Concat("Looking for rescue bed for ", targetA));
                PetBed bed = RestAIUtility_Animal.FindBedForAnimal(pawn, targetA as TameablePawn, false, false, false) as PetBed;
                return new Job(RestAIUtility_Animal.RescueJobDef, targetA, bed) { maxNumToCarry = 1 };
            }
            else
                return new Job(JobDefOf.Rescue, targetA, base.FindBed(pawn, targetA)) { maxNumToCarry = 1 };
        }

        // Properties
        public override PathEndMode PathEndMode
        {
            get
            {
                return PathEndMode.OnCell;
            }
        }
        public override ThingRequest PotentialWorkThingRequest
        {
            get
            {
                return ThingRequest.ForGroup(ThingRequestGroup.Pawn);
            }
        }
    }
}
