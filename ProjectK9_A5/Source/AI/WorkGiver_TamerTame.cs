using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;
using Verse.AI;
using RimWorld;

namespace ProjectK9.AI
{
    class WorkGiver_TamerTame : WorkGiver
    {
        private DesignationDef tameDesDef = DefDatabase<DesignationDef>.GetNamed("Tame");
        private JobDef         tameJobDef = DefDatabase<JobDef        >.GetNamed("Tame");

        public WorkGiver_TamerTame(WorkGiverDef giverDef) : base(giverDef)
        {
        }

        public override IEnumerable<Thing> PotentialWorkThingsGlobal
        {
            get
            {
                foreach (Designation designation in Find.DesignationManager.DesignationsOfDef(tameDesDef))
                {
                    yield return designation.target.Thing;
                }
            }
        }

        public override Job StartingJobForOn(Pawn pawn, Thing t)
        {
            TameablePawn target = t as TameablePawn;
            if (target == null) 
                return null;
            if (!ReservationUtility.CanReserve(pawn, target, ReservationType.Total))
                return null;
            if (Find.DesignationManager.DesignationOn(t, tameDesDef) == null)
                return null;

            return new Job(tameJobDef, target);
        }
 
    }
}
