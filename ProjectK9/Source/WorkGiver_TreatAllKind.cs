using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using Verse.AI;

namespace ProjectK9
{
    public class WorkGiver_TreatAllKind : WorkGiver_Treat
    {
        public override bool HasJobOnThing(Pawn pawn, Thing t)
        {
            Pawn p = t as Pawn;
            return ((((p != null) && (p.RaceProps.Humanlike || p is TameablePawn)) && ((p.playerController == null) || !p.playerController.Drafted)) && ((p.InBed() && p.health.ShouldGetTreatment) && pawn.CanReserve(p, 1)));
        }
    }
}
