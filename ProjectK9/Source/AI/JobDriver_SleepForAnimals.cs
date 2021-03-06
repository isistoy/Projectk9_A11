﻿using System;
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
        private bool layingInt;

        public new bool Asleep
        {
            get
            {
                if (pawn is TameablePawn)
                {
                    return this.sleepingInt;
                }
                else
                    return base.Asleep;
            }
        }

        public JobDriver_SleepForAnimals()
        {
        }

        public override PawnPosture Posture
        {
            get
            {
                if (pawn is TameablePawn)
                {
                    return (!this.layingInt ? PawnPosture.Standing : PawnPosture.LayingAny);
                }
                else
                {
                    return base.Posture;
                }
            }
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            if (pawn is TameablePawn)
            {
                if (pawn.jobs.curJob.targetA.HasThing)
                {
                    this.FailOnDespawned<JobDriver_LayDown>(TargetIndex.A);
                    this.FailOn<JobDriver_LayDown>(() => { return ((Building_Bed)pawn.CurJob.GetTarget(TargetIndex.A).Thing).IsBurning(); });
                    this.FailOnNonMedicalBedNotOwned<JobDriver_LayDown>(TargetIndex.A, TargetIndex.None);
                    //this.FailOn<JobDriver_LayDown>(() => { return pawn.health.CanUseMedicalBed; }); //&& ((Building_Bed)TargetThingA).Medical;
                    this.FailOn<JobDriver_LayDown>(() => { return ((((TameablePawn)pawn).IsColonyPet && !CurJob.ignoreForbidden) && !pawn.Downed) && TargetThingA.IsForbidden(pawn); });

                    yield return Toils_Bed.ClaimBedIfNonMedical(TargetIndex.A, TargetIndex.None);
                    yield return Toils_Reserve.Reserve(TargetIndex.A, 1);
                }

                yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
                Toil sleep = new Toil();
                sleep.tickAction = new Action(tickAction);
                sleep.defaultCompleteMode = ToilCompleteMode.Never;
                yield return sleep;
            }
            else
            {
                IEnumerator<Toil> toils = base.MakeNewToils().GetEnumerator();
                while (toils.MoveNext())
                {
                    yield return toils.Current;
                }
                toils.Dispose();
            }
        }

        private void tickAction()
        {
            Building_Bed building_Bed = (Building_Bed)pawn.jobs.curJob.GetTarget(TargetIndex.A).Thing;
            this.layingInt = true;
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

            if (((Find.TickManager.TicksGame % tickInterval == 0)
                && (pawn.health.hediffSet.GetNaturallyHealingInjuredParts().Any<BodyPartRecord>() && (pawn.needs.food != null)))
                && !pawn.needs.food.Starving)
            {
                //Log.Message(string.Concat(pawn, " is healing..."));
                BodyPartRecord part = pawn.health.hediffSet.GetNaturallyHealingInjuredParts().RandomElement<BodyPartRecord>();
                List<HediffDef> healHediff = DefDatabase<HediffDef>.AllDefs.Where(def => def.naturallyHealed).ToList<HediffDef>();
                BodyPartDamageInfo info = new BodyPartDamageInfo(part, false, healHediff);
                pawn.TakeDamage(new DamageInfo(DamageDefOf.HealInjury, amountToHeal, null, new BodyPartDamageInfo?(info), null));
                if ((!pawn.health.ShouldGetTreatment && !pawn.health.WantsToRemainInBedForMedicine) && !pawn.health.hediffSet.GetNaturallyHealingInjuredParts().Any<BodyPartRecord>())
                {
                    object[] args = new object[] { pawn.LabelBaseShort };
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
            if (((building_Bed != null) /*&& !building_Bed.Medical*/) && (building_Bed.owner != pawn))
            {
                if (pawn.Downed)
                {
                    pawn.Position = GenCellFinder.RandomStandableClosewalkCellNear(pawn.Position, 1);
                }
                pawn.jobs.EndCurrentJob(JobCondition.Incompletable);
            }
            else if (pawn.IsHashIntervalTick(211) && ShouldStopSleeping(pawn, building_Bed))
            {
                //Log.Message(string.Concat(pawn, " trying to stop sleeping"));
                pawn.jobs.CheckForJobOverride();
            }
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
                    if (pawn.needs.food.CurCategory >= HungerCategory.UrgentlyHungry)
                    {
                        return true;
                    }
                    if (!pawn.health.hediffSet.HasNaturallyHealingInjuries)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
