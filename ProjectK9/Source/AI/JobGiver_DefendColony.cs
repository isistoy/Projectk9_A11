using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;
using Verse.AI;
using RimWorld;

namespace ProjectK9.AI
{
    public class JobGiver_DefendColony : ThinkNode_JobGiver
    {
        protected override Job TryGiveTerminalJob(Pawn pawn)
        {
            //Log.Message(string.Concat(pawn, " looking for any colony threat"));
            ThingRequest pawnReq = ThingRequest.ForGroup(ThingRequestGroup.Pawn);
            Predicate<Thing> hostilePredicate = t => 
            {
                Pawn hostile = t as Pawn;
                return ( ((TameablePawn)pawn).IsColonyPet && !hostile.Dead && !hostile.Downed && 
                    (hostile.Faction.HostileTo(Faction.OfColony) || hostile.IsPrisonerOfColony) );
            };
            
            Pawn closestEnemy = 
                GenClosest.ClosestThingReachable(pawn.Position, pawnReq, 
                    PathEndMode.OnCell, TraverseParms.For(pawn), 100f, hostilePredicate) as Pawn;

            if (closestEnemy == null || closestEnemy == pawn || !GenSight.LineOfSight(pawn.Position, closestEnemy.Position))
                return null;

            Log.Message(string.Concat(pawn," found threat to colony: ", closestEnemy));
            return new Job(JobDefOf.AttackMelee)
            {
                targetA=closestEnemy,
                expiryInterval = 200,
            };
        }
    }
}
