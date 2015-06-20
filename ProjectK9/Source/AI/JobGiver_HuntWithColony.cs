using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;
using Verse.AI;
using RimWorld;

namespace ProjectK9.AI
{
    public class JobGiver_HuntWithColony : ThinkNode_JobGiver
    {

        protected override Job TryGiveTerminalJob(Pawn pawn)
        {
            // Only hunt with colonists, not with group
            TameablePawn pet = pawn as TameablePawn;
            if (pet != null)
            {
                if (!pet.IsColonyPet)
                    Log.Error(string.Concat(pet, " is not a colony Pet, in HuntWithColony"));
            }
            else
            {
                Log.Error(string.Concat(pawn, " is not a pet, in HuntWithColony"));
                return null;
            }

            JobDef killJobDef = DefDatabase<JobDef>.GetNamed("Kill");

            IEnumerable<Pawn> colonists = Find.ListerPawns.FreeColonists;
            foreach (Pawn colonist in colonists)
            {
                if (colonist.jobs.curJob != null && colonist.jobs.curJob.def == JobDefOf.Hunt && colonist.jobs.curJob.targetA != null)
                {
                    Log.Message(string.Concat(pet, " hunting prey ", colonist.jobs.curJob.targetA, " with colonist ", colonist));
                    return new Job(JobDefOf.AttackMelee, colonist.jobs.curJob.targetA) { killIncappedTarget = true };
                }
            }
            return null;
        }
    }
}
