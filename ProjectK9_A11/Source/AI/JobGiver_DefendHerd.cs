using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;
using Verse.AI;
using RimWorld;

namespace ProjectK9.AI
{
    class JobGiver_DefendHerd : ThinkNode_JobGiver
    {


        protected override Job TryGiveTerminalJob(Pawn pawn)
        {
            Pawn threat = pawn.mindState.meleeThreat;
            Pawn targetOfThreat = pawn;

            if (threat == null)
            {
                IEnumerable<Pawn> herdMembers = HerdUtility.FindHerdMembers(pawn);
                foreach(Pawn herdMember in herdMembers)
                {
                    if (herdMember.mindState.meleeThreat != null)
                    {
                        threat = herdMember.mindState.meleeThreat;
                        pawn.mindState.meleeThreat = threat;
                        targetOfThreat = herdMember;
                        break;
                    }
                }
            }
                

            if (threat == null || threat.Dead || threat.Downed
                || (targetOfThreat.mindState.lastMeleeThreatHarmTick - Find.TickManager.TicksGame) > 300
                || (targetOfThreat.Position - threat.Position).LengthHorizontalSquared > HerdUtility.HERD_DISTANCE 
                || !GenSight.LineOfSight(pawn.Position, threat.Position))
            {
                pawn.mindState.meleeThreat = null;
                return null;
            }
            else
            {
                return new Job(JobDefOf.AttackMelee)
                {
                    targetA = threat,
                    maxNumMeleeAttacks = 1,
                    expiryInterval = 200
                };
            }
        }
    }
}
