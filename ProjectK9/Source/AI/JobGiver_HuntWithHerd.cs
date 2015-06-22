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
            TameablePawn animal = pawn as TameablePawn;
            if (animal.IsColonyPet)
            {
                Log.Error(string.Concat(animal, " is a colony Pet, in HuntWithHerd"));
                return null;
            }

            JobDef huntJobDef = DefDatabase<JobDef>.GetNamed("HuntForAnimals");

            Pawn targetA = null;
            IEnumerator<Pawn> enumerator = HerdUtility_Pets.FindHerdMembers(pawn).GetEnumerator();
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
                            Log.Message(string.Concat(animal, " found prey ", targetA, " hunted by ", current));
                        }
                    }
                }
            }
            finally
            {
                if (enumerator==null)
                { }
                enumerator.Dispose();
            }
            if (targetA != null)
            {
                Log.Message(string.Concat(animal, " hunting prey ", targetA));
                return new Job(huntJobDef, targetA) { checkOverrideOnExpire = true, expiryInterval = 500 };
            }
            
            return null;
        }
    }
}
