using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Verse;
using Verse.AI;

namespace ProjectK9.AI
{
    public class JobDriver_KillUnreserved : JobDriver
    {
        public JobDriver_KillUnreserved()
        {
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            
            ToilFailConditions.EndOnDespawned<JobDriver_KillUnreserved>(this, TargetIndex.A, JobCondition.Succeeded);
            this.FailOn(hunterIsKilled);
            yield return Toils_Combat.TrySetJobToUseAttackVerb();
            Toil gotoPosition = Toils_Combat.GotoCastPosition(TargetIndex.A);
            yield return gotoPosition;
            Toil jump = Toils_Jump.JumpIfTargetNotHittable(TargetIndex.A, gotoPosition);
            yield return jump;
            Log.Message(pawn + " trying to kill " + TargetA);
            yield return Toils_Combat.TrySetJobToUseAttackVerb();
            yield return Toils_Combat.CastVerb(TargetIndex.A);
            yield return Toils_Jump.Jump(jump);
            
        }

        private bool hunterIsKilled()
        {
            return pawn.Dead || pawn.Downed || pawn.HitPoints == 0;
        }
    }
}
