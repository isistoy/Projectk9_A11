using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;
using RimWorld;
using Verse.AI;

namespace ProjectK9.AI
{
    public class JobGiver_HuntWithHerd : ThinkNode_JobGiver
    {
        public const int HUNT_DISTANCE = 35 * 35;
        protected override Job TryGiveTerminalJob(Pawn pawn)
        {
            JobDef huntJobDef = FoodAIUtility_Animals.GetHuntForAnimalsJobDef();
            if ((pawn.jobs.curJob == null) || ((pawn.jobs.curJob.def != huntJobDef) && pawn.jobs.curJob.checkOverrideOnExpire))
            {
                TameablePawn animal = pawn as TameablePawn;
                if (animal.IsColonyPet)
                {
                    Log.Error(string.Concat(animal, " is a colony Pet, in HuntWithHerd"));
                    return null;
                }                              

                Pawn targetA = null;
                IEnumerator<Pawn> enumerator = HerdAIUtility_Pets.FindHerdMembers(pawn).GetEnumerator();
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
                            if ((huntTarget != null)
                                && pawn.CanReach(huntTarget, PathEndMode.OnCell, Danger.Deadly)
                                && !(huntTarget.Thing is Corpse)
                                && !((Pawn)huntTarget).Dead
                                && (pawn.Position - huntTarget.Center).LengthHorizontalSquared <= HUNT_DISTANCE)
                            {
                                targetA = (Pawn)huntTarget;
                                if (targetA != null)
                                {
                                    Log.Message(string.Concat(animal, " hunting prey ", targetA));
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
                        }
                    }
                }
                finally
                {
                    enumerator.Dispose();
                }                
            }            
            return null;
        }
    }
}
