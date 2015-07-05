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
        //public const int HUNT_DISTANCE = 35 * 35;
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
                IEnumerable<Pawn> colonists = Find.ListerPawns.FreeColonists;
                foreach (Pawn colonist in colonists)
                {
                    if (!colonist.Downed
                        && (colonist.jobs.curJob != null)
                        && (colonist.jobs.curJob.def == JobDefOf.Hunt))
                    {
                        TargetInfo huntTarget = colonist.jobs.curJob.targetA;
                        if ((huntTarget != null)
                            && pawn.CanReach(huntTarget, PathEndMode.OnCell, Danger.Deadly)
                            && !(huntTarget.Thing is Corpse)
                            && !((Pawn)huntTarget).Dead)
                        //&& (pawn.Position - huntTarget.Center).LengthHorizontalSquared <= HUNT_DISTANCE)
                        {
                            targetA = (Pawn)huntTarget;
                            Log.Message(string.Concat(pet, " found prey ", targetA, " hunted by ", colonist));
                        }
                    }
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
