using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Verse;
using Verse.AI;
using RimWorld;

namespace ProjectK9.AI
{
    class JobDriver_SleepForAnimals : JobDriver_LayDown
    {
        private bool sleepingInt;
        private bool layingDownInt;

        public new bool Asleep
        {
            get
            {
                return this.sleepingInt;
            }
        }

        public JobDriver_SleepForAnimals()
        {
        }

        public override PawnPosture Posture
        {
            get
            {
                return (!this.layingDownInt ? PawnPosture.Standing : PawnPosture.LayingAny);
            }
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Thing thing = pawn.jobs.curJob.targetA.HasThing ? pawn.jobs.curJob.targetA.Thing : null;
            if (thing != null && thing is Building_Bed)
            {
                Log.Message("Possible bed found");
                this.FailOnDespawned<JobDriver_LayDown>(TargetIndex.A);
                this.FailOn<JobDriver_LayDown>(() => { return ((Building_Bed)pawn.CurJob.GetTarget(TargetIndex.A).Thing).IsBurning(); });
                this.FailOnNonMedicalBedNotOwned<JobDriver_LayDown>(TargetIndex.A, TargetIndex.None);
                this.FailOn<JobDriver_LayDown>(() => { return pawn.health.CanUseMedicalBed; }); //&& ((Building_Bed)TargetThingA).Medical;
                this.FailOn<JobDriver_LayDown>(() => { return ((((TameablePawn)pawn).IsColonyPet && !pawn.CurJob.ignoreForbidden) && !pawn.Downed) && TargetThingA.IsForbidden(pawn); });

                yield return Toils_Bed.ClaimBedIfNonMedical(TargetIndex.A, TargetIndex.None);
            }

            yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);

            Toil sleep = new Toil();
            sleep.tickAction = new Action(tickAction);
            sleep.defaultCompleteMode = ToilCompleteMode.Never;

            yield return sleep;

        }

        private void tickAction()
        {
            Building_Bed building_Bed = (Building_Bed)pawn.jobs.curJob.GetTarget(TargetIndex.A).Thing;
            this.layingDownInt = true;
            if (!this.sleepingInt)
            {
                if (pawn.needs.rest.CurLevel < 0.55f)
                {
                    sleepingInt = true;
                }
            }
            else if (pawn.needs.rest.CurLevel >= 1f)
            {
                sleepingInt = false;
            }
            if (sleepingInt)
            {
                float RestEffectiveness = building_Bed == null ? 0.75f : building_Bed.GetStatValue(StatDefOf.BedRestEffectiveness);
                pawn.needs.rest.TickResting(RestEffectiveness);
            }
            int amountToHeal = building_Bed != null ? 6 : 3;
            int tickInterval = building_Bed != null ? building_Bed.def.building.bed_healTickInterval : 1000;

            if ((((building_Bed == null) && (Find.TickManager.TicksGame % tickInterval == 0))
                && (pawn.health.hediffSet.GetNaturallyHealingInjuredParts().Any<BodyPartRecord>() && (pawn.needs.food != null)))
                && !pawn.needs.food.Starving)
            {
                BodyPartRecord part = pawn.health.hediffSet.GetNaturallyHealingInjuredParts().RandomElement<BodyPartRecord>();
                List<HediffDef> healHediff = DefDatabase<HediffDef>.AllDefs.Where(def => def.naturallyHealed).ToList<HediffDef>();
                BodyPartDamageInfo info = new BodyPartDamageInfo(part, false, healHediff);
                pawn.TakeDamage(new DamageInfo(DamageDefOf.HealInjury, amountToHeal, null, new BodyPartDamageInfo?(info), null));
                if ((!pawn.health.ShouldGetTreatment && !pawn.health.WantsToRemainInBedForMedicine) && !pawn.health.hediffSet.GetNaturallyHealingInjuredParts().Any<BodyPartRecord>())
                {
                    object[] args = new object[] { pawn.LabelCap };
                    Messages.Message("MessageFullyHealed".Translate(args), MessageSound.Benefit);
                }
            }

            if (pawn.IsHashIntervalTick(100))
            {
                if (sleepingInt)
                {
                    MoteThrower.ThrowDrift(pawn.Position, ThingDefOf.Mote_SleepZ);
                }
                if (pawn.health.hediffSet.GetNaturallyHealingInjuredParts().Any<BodyPartRecord>())
                {
                    MoteThrower.ThrowDrift(pawn.Position, ThingDefOf.Mote_HealingCross);
                }
            }
            if (((building_Bed != null) && !building_Bed.Medical) && (building_Bed.owner != pawn))
            {
                if (pawn.Downed)
                {
                    pawn.Position = GenCellFinder.RandomStandableClosewalkCellNear(pawn.Position, 1);
                }
                pawn.jobs.EndCurrentJob(JobCondition.Incompletable);
            }
            else if (pawn.IsHashIntervalTick(211) && ShouldStopSleeping(pawn, building_Bed))
            {
                pawn.jobs.CheckForJobOverride();
            }

            //if (building_Bed != null && building_Bed.owner != pawn)
            //{
            //    if (pawn.Downed)
            //        pawn.Position = GenCellFinder.RandomStandableClosewalkCellNear(pawn.Position, 1);
            //    pawn.jobs.EndCurrentJob(JobCondition.Incompletable);
            //}
            //else
            //{


            //    --this.ticksToSleepZ;
            //    if (this.ticksToSleepZ <= 0)
            //    {
            //        if (!ShouldStopSleeping(pawn, building_Bed))
            //            MoteThrower.ThrowDrift(pawn.Position,ThingDefOf.Mote_SleepZ);
            //        if (pawn.health.summaryHealth.SummaryHealthPercent < 0.99f)
            //            MoteThrower.ThrowDrift(pawn.Position, ThingDefOf.Mote_HealingCross);
            //        this.ticksToSleepZ = 100;
            //    }
            //    if (pawn.Downed || !ShouldStopSleeping(pawn, building_Bed) || pawn.health.summaryHealth.SummaryHealthPercent < 0.99f)
            //        return;
            //    pawn.jobs.EndCurrentJob(JobCondition.Incompletable);
            //}
        }

        public bool ShouldStopSleeping(Pawn pawn, Building_Bed bed)
        {
            if (!pawn.Downed)
            {
                if (pawn.needs.rest.CurLevel >= 0.8f)
                {
                    return true;
                }

                if (!this.sleepingInt)
                {
                    if (bed == null)
                    {
                        return true;
                    }
                    if (((pawn.needs.food.CurCategory >= HungerCategory.UrgentlyHungry) && !pawn.CurJob.playerForced) && MayGetUpVoluntarily(pawn))
                    {
                        return true;
                    }
                    if (!pawn.health.hediffSet.HasNaturallyHealingInjuries
                        && !pawn.health.hediffSet.HasTreatableInjury)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
