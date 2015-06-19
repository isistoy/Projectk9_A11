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
            if (pet == null)
            {
                Log.Message("Not a TameablePawn in HuntWithColony");
                return null;
            }
            else
            {
                if (!pet.IsColonyPet)
                    Log.Error("Not a colony Pet in HuntWithColony");
            }
            
            Boolean isHunter = !pawn.story.DisabledWorkTypes.Contains(WorkTypeDefOf.Hunting);
            JobDef killJobDef = DefDatabase<JobDef>.GetNamed("Kill");
            
            IEnumerable<Pawn> colonists = Find.ListerPawns.FreeColonists;
            if (isHunter)
            {
                foreach (Pawn colonist in colonists)
                {
                    if (colonist.jobs.curJob != null && colonist.jobs.curJob.def == JobDefOf.Hunt)
                        return new Job(killJobDef, colonist.jobs.curJob.targetA);
                }
            }            
            //IEnumerable<Pawn> pets = Find.ListerPawns.AllPawns.Where(t => (t is TameablePawn) && ((TameablePawn)t).IsColonyPet );

            //foreach (Pawn comp in pets) {
            //    if (HerdUtility.isInHerd(pawn, comp))
            //    {
            //        if (comp.jobs.curJob != null && comp.jobs.curJob.def == killJobDef)
            //            return new Job(killJobDef, comp.jobs.curJob.targetA);
            //    }
            //}
            return null;
        }
    }
}
