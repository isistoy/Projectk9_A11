﻿using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Verse;
using Verse.AI;
using RimWorld;

namespace ProjectK9
{
    public class WorkGiver_TamerTame : WorkGiver
    {
        public WorkGiver_TamerTame()
        {
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t)
        {
            TameablePawn pet = t as TameablePawn;
            if (pet == null)
                return false;
            if (!pawn.CanReserve(t, 1))
                return false;
            if (Find.DesignationManager.DesignationOn(t, TamePawnUtility.GetTameDesDef()) == null)
                return false;
            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing t)
        {
            return new Job(TamePawnUtility.GetTameJobDef(), t);
        }

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn Pawn)
        {
            foreach (Designation designation in Find.DesignationManager.DesignationsOfDef(TamePawnUtility.GetTameDesDef()))
            {
                yield return designation.target.Thing;
            }
        }
        
        public override ThingRequest PotentialWorkThingRequest
        {
            get
            {
                if (base.def.singleBillGiverDef != null)
                {
                    return ThingRequest.ForDef(base.def.singleBillGiverDef);
                }
                return ThingRequest.ForGroup(ThingRequestGroup.PotentialBillGiver);
            }
        }

        public override PathEndMode PathEndMode
        {
            get
            {
                return PathEndMode.OnCell;
            }
        }

    }
}