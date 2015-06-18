using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;
using Verse.AI;

namespace ProjectK9.AI
{
    class JobGiver_HuntWithHerd : ThinkNode_JobGiver
    {

        protected override Job TryGiveTerminalJob(Pawn pawn)
        {
            JobDef killJobDef = DefDatabase<JobDef>.GetNamed("Kill"); 
            IEnumerable<Pawn> herdMembers = HerdUtility.FindHerdMembers(pawn);
            foreach (Pawn herdMember in herdMembers)
            {
                if (herdMember.jobs.curJob != null && herdMember.jobs.curJob.def == killJobDef)
                {
                    return new Job(killJobDef, herdMember.jobs.curJob.targetA);
                }
            }
            return null;
        }
    }
}
