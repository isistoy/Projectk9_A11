using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;
using Verse.AI;
using RimWorld;

namespace ProjectK9.AI
{
    public class JobGiver_DefendHerd : ThinkNode_JobGiver
    {
        protected override Job TryGiveTerminalJob(Pawn pawn)
        {
            JobDef huntJobDef = FoodAIUtility_Animals.GetHuntForAnimalsJobDef();
            if ((pawn.jobs.curJob == null) || ((pawn.jobs.curJob.def != huntJobDef) && pawn.jobs.curJob.checkOverrideOnExpire))
            {
                //Log.Message(string.Concat(pawn, " looking for any herd threat"));
                Pawn threat = pawn.mindState.meleeThreat;
                Pawn targetOfThreat = pawn;

                if (threat == null)
                {
                    IEnumerable<Pawn> herdMembers = HerdAIUtility_Pets.FindHerdMembers(pawn);
                    // Remplacer foreach par un itérateur avec MoveNext
                    foreach (Pawn herdMember in herdMembers)
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
                    || (targetOfThreat.Position - threat.Position).LengthHorizontalSquared > HerdAIUtility_Pets.HERD_DISTANCE
                    || !GenSight.LineOfSight(pawn.Position, threat.Position))
                {
                    pawn.mindState.meleeThreat = null;
                    return null;
                }
                else
                {
                    Log.Message(string.Concat(pawn, " found threat to herd: ", threat));
                    return new Job(huntJobDef, threat);
                } 
            }
            return null;
        }
    }
}
