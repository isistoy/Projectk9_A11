using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;
using RimWorld;
using Verse.AI;

namespace ProjectK9.AI
{
    class JobGiver_HuntWithHerd : ThinkNode_JobGiver
    {

        protected override Job TryGiveTerminalJob(Pawn pawn)
        {
            IEnumerable<Pawn> herdMembers = HerdUtility_Pets.FindHerdMembers(pawn);
            foreach (Pawn herdMember in herdMembers)
            {
                if (herdMember.jobs.curJob != null && herdMember.jobs.curJob.def == JobDefOf.AttackMelee)
                {
                    Log.Message(string.Concat(pawn," trying to hunt with ", herdMember));
                    TargetInfo target = herdMember.jobs.curJob.targetA;
                    return new Job(JobDefOf.AttackMelee, target);
                }
            }
            return null;
        }
    }
}
