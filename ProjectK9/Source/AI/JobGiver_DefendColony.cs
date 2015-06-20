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
            ThingRequest pawnReq = ThingRequest.ForGroup(ThingRequestGroup.Pawn);
            Predicate<Thing> hostilePredicate = t => 
            {
                Pawn hostile = t as Pawn;
                TameablePawn pet = pawn as TameablePawn;
                if (pet.IsColonyPet
                    && (hostile.Faction.HostileTo(Faction.OfColony) || hostile.IsPrisonerOfColony)
                    && pawn.Position.CanReach(hostile.Position, PathEndMode.OnCell, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false)))
                {
                    return true;
                }
                return false;
            };
            
            Pawn closestEnemy = 
                GenClosest.ClosestThingReachable(pawn.Position, pawnReq, 
                    PathEndMode.OnCell, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), 100f, hostilePredicate) as Pawn;

            if (closestEnemy == null || closestEnemy == pawn)
                return null;

            Log.Warning(pawn + " found threat to colony: " + closestEnemy);
            return new Job(JobDefOf.AttackMelee)
            {
                targetA=closestEnemy,
                maxNumMeleeAttacks = 1,
                expiryInterval = 200
            };
        }
    }
}
