using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Verse;
using Verse.AI;
using RimWorld;

namespace ProjectK9
{
    public class JobDriver_Tame : JobDriver
    {
        public JobDriver_Tame()
        {
        }

        protected TameablePawn Tamee
        {
            get { return (TameablePawn)base.CurJob.targetA.Thing; }
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyed(TargetIndex.A);
            this.FailOnThingMissingDesignation(TargetIndex.A, DefDatabase<DesignationDef>.GetNamed("Tame"));
            this.FailOnDespawned(TargetIndex.A);
            yield return Toils_Reserve.Reserve(TargetIndex.A);
            yield return Toils_Tameable.GotoTameable(pawn, Tamee);
            yield return Toils_Tameable.ConvinceTameable(pawn, Tamee);
            yield return Toils_Tameable.GotoTameable(pawn, Tamee);
            yield return Toils_Tameable.ConvinceTameable(pawn, Tamee);
            yield return Toils_Tameable.GotoTameable(pawn, Tamee);
            yield return Toils_Tameable.ConvinceTameable(pawn, Tamee);
            yield return Toils_Tameable.GotoTameable(pawn, Tamee);
            yield return Toils_Tameable.ConvinceTameable(pawn, Tamee);
            yield return Toils_Tameable.GotoTameable(pawn, Tamee);
            yield return Toils_Tameable.SetTameableLastInteractTime(TargetIndex.A);
            Log.Message("taming: recruit dog");
            yield return Toils_Tameable.TryTameTameable(TargetIndex.A);
        }
    }
}
