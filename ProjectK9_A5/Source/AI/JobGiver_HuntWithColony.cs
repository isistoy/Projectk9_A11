using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;
using Verse.AI;
using RimWorld;

namespace ProjectK9.AI
{
    class JobGiver_HuntWithColony : ThinkNode_JobGiver
    {

        protected override Job TryGiveTerminalJob(Pawn pawn)
        {
            WorkTypeDef huntingDef = DefDatabase<WorkTypeDef>.GetNamed("Hunting");
            Boolean isHunter = pawn.workSettings.ActiveWorkTypesByPriority.Contains(huntingDef);
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
            Faction petFaction = Find.FactionManager.FirstFactionOfDef(FactionDef.Named("ColonyPets"));
            IEnumerable<Pawn> pets = Find.ListerPawns.AllPawns.Where(pet => pet.Faction == petFaction);

            foreach (Pawn pet in pets) {
                if (HerdUtility.isInHerd(pawn, pet))
                {
                    if (pet.jobs.curJob != null && pet.jobs.curJob.def == killJobDef)
                        return new Job(killJobDef, pet.jobs.curJob.targetA);
                }
            }
            return null;
        }
    }
}
