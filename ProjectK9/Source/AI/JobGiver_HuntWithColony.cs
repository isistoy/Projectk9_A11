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
            JobDef huntJobDef = FoodAIUtility_Animals.GetHuntForAnimalsJobDef();
            if ((pawn.jobs.curJob == null) || ((pawn.jobs.curJob.def != huntJobDef) && pawn.jobs.curJob.checkOverrideOnExpire))
            {
                // Only hunt with colonists, not with group
                TameablePawn pet = pawn as TameablePawn;
                if (!pet.IsColonyPet)
                {
                    Log.Error(string.Concat(pet, " is not a colony Pet, in HuntWithColony"));
                    return null;
                }

                Pawn targetA = null;
                IEnumerator<Pawn> enumerator = Find.ListerPawns.FreeColonists.GetEnumerator();
                try
                {
                    while (enumerator.MoveNext())
                    {
                        Pawn current = enumerator.Current;
                        if (!current.Downed
                            && (current.jobs.curJob != null)
                            && (current.jobs.curJob.def == huntJobDef))
                        {
                            TargetInfo huntTarget = current.jobs.curJob.targetA;
                            if (pawn.CanReach(huntTarget, PathEndMode.OnCell, Danger.Deadly)
                                && !(huntTarget.Thing is Corpse)
                                && ((Pawn)huntTarget).Downed
                                && ((Pawn)huntTarget).Dead)
                            {
                                targetA = (Pawn)huntTarget;
                                Log.Message(string.Concat(pet, " found prey ", targetA, " hunted by ", current));
                            }
                        }
                    }
                }
                finally
                {
                    enumerator.Dispose();
                }
                if (targetA != null)
                {
                    Log.Message(string.Concat(pet, " hunting prey ", targetA));
                    return new Job(huntJobDef)
                    {
                        targetA = targetA,
                        maxNumMeleeAttacks = 4,
                        killIncappedTarget = true,
                        checkOverrideOnExpire = true,
                        expiryInterval = 500
                    };
                }
            }

            return null;
        }
    }
}
