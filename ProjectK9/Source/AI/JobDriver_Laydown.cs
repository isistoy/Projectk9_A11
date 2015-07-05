using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using Verse.AI;

namespace ProjectK9.AI
{
    public class JobDriver_LayDown : JobDriver
    {
        // Fields
        private bool asleepInt;
        private const TargetIndex BedIndex = TargetIndex.A;
        private const int GetUpCheckInterval = 0xd3;
        private const float GroundRestEffectiveness = 0.8f;
        private bool layingDownInt;
        private const int TicksBetweenSleepZs = 100;

        // Methods
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.LookValue<bool>(ref this.asleepInt, "asleep", false, false);
        }

        public override string GetReport()
        {
            if (this.asleepInt)
            {
                return "ReportSleeping".Translate();
            }
            return "ReportResting".Translate();
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            if (!this.pawn.jobs.curJob.targetA.HasThing)
            {
                yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
            }
            else
            {
                this.FailOnDespawned<JobDriver_LayDown>(TargetIndex.A);
                this.FailOn<JobDriver_LayDown>(() => ((Thing)((Building_Bed)this.pawn.CurJob.GetTarget(TargetIndex.A).Thing)).IsBurning());
                this.FailOn<JobDriver_LayDown>(() => (((Building_Bed)this.pawn.CurJob.GetTarget(TargetIndex.A).Thing).ForPrisoners != this.pawn.IsPrisoner));
                this.FailOnNonMedicalBedNotOwned<JobDriver_LayDown>(TargetIndex.A, TargetIndex.None);
                this.FailOn<JobDriver_LayDown>(() => (!this.pawn.health.CanUseMedicalBed && ((Building_Bed)this.TargetThingA).Medical));
                this.FailOn<JobDriver_LayDown>(() => (((this.pawn.IsColonist && !this.CurJob.ignoreForbidden) && !this.pawn.Downed) && this.TargetThingA.IsForbidden(this.pawn)));
                yield return Toils_Bed.ClaimBedIfNonMedical(TargetIndex.A, TargetIndex.None);
                yield return Toils_Reserve.Reserve(TargetIndex.A, 1);
                yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.OnCell);
            }

            Toil interact = new Toil();
            interact.tickAction = () => actionInteract(interact);
            interact.defaultCompleteMode = ToilCompleteMode.Never;
            yield return interact;

        }

        private void actionInteract(Toil interact)
        {
            Pawn actor = interact.actor;
            Building_Bed thing = (Building_Bed)actor.jobs.curJob.GetTarget(TargetIndex.A).Thing;
            if (!(actor is TameablePawn))
                actor.GainComfortFromCellIfPossible();
            this.layingDownInt = true;
            if (!this.asleepInt)
            {
                if (actor.needs.rest.CurLevel < 0.75f)
                {
                    this.asleepInt = true;
                }
            }
            else if (actor.needs.rest.CurLevel >= 1f)
            {
                this.asleepInt = false;
            }
            if (this.asleepInt)
            {
                float num;
                if (!(actor is TameablePawn))
                {
                    if ((Find.TickManager.TicksGame % 750) == 0)
                    {
                        if (RoomQuery.RoomAt(actor.Position).TouchesMapEdge)
                        {
                            actor.needs.mood.thoughts.TryGainThought(ThoughtDefOf.SleptOutside);
                        }
                        if ((thing == null) || (thing.CostListAdjusted().Count == 0))
                        {
                            actor.needs.mood.thoughts.TryGainThought(ThoughtDefOf.SleptOnGround);
                        }
                        if (GenTemperature.GetTemperatureForCell(actor.Position) < actor.GetStatValue(StatDefOf.ComfyTemperatureMin, true))
                        {
                            actor.needs.mood.thoughts.TryGainThought(ThoughtDefOf.SleptInCold);
                        }
                        if (GenTemperature.GetTemperatureForCell(actor.Position) > actor.GetStatValue(StatDefOf.ComfyTemperatureMax, true))
                        {
                            actor.needs.mood.thoughts.TryGainThought(ThoughtDefOf.SleptInHeat);
                        }
                    } 
                }
                if ((thing != null) && thing.def.statBases.StatListContains(StatDefOf.BedRestEffectiveness))
                {
                    num = thing.GetStatValue(StatDefOf.BedRestEffectiveness, true);
                }
                else
                {
                    num = 0.8f;
                }
                float num2 = (this.pawn.health.capacities.GetEfficiency(PawnCapacityDefOf.BloodPumping) * this.pawn.health.capacities.GetEfficiency(PawnCapacityDefOf.Metabolism)) * this.pawn.health.capacities.GetEfficiency(PawnCapacityDefOf.Breathing);
                num = (0.7f * num) + ((0.3f * num) * num2);
                actor.needs.rest.TickResting(num);
            }
            int amountToHeal = thing != null ? 6 : 3;
            int tickInterval = thing != null ? thing.def.building.bed_healTickInterval : 1000;
            bool shouldHeal = ((((thing != null) || actor is TameablePawn) && ((Find.TickManager.TicksGame % tickInterval) == 0)) 
                && (actor.health.hediffSet.GetNaturallyHealingInjuredParts().Any<BodyPartRecord>() && (actor.needs.food != null))) 
                && !actor.needs.food.Starving;
            if (shouldHeal)
            {;
                BodyPartRecord part = actor.health.hediffSet.GetNaturallyHealingInjuredParts().RandomElement<BodyPartRecord>();
                List<HediffDef> healHediff = DefDatabase<HediffDef>.AllDefs.Where<HediffDef>(def => def.naturallyHealed).ToList<HediffDef>();
                BodyPartDamageInfo info = new BodyPartDamageInfo(part, false, healHediff);
                actor.TakeDamage(new DamageInfo(DamageDefOf.HealInjury, 1, null, new BodyPartDamageInfo?(info), null));
                if ((!actor.health.ShouldGetTreatment && !actor.health.WantsToRemainInBedForMedicine) && !actor.health.hediffSet.GetNaturallyHealingInjuredParts().Any<BodyPartRecord>())
                {
                    object[] args = new object[] { actor.LabelCap };
                    Messages.Message("MessageFullyHealed".Translate(args), MessageSound.Benefit);
                }
            }
            if (this.pawn.IsHashIntervalTick(100))
            {
                if (this.asleepInt)
                {
                    MoteThrower.ThrowDrift(actor.Position, ThingDefOf.Mote_SleepZ);
                }
                if (actor.health.hediffSet.GetNaturallyHealingInjuredParts().Any<BodyPartRecord>())
                {
                    MoteThrower.ThrowDrift(actor.Position, ThingDefOf.Mote_HealingCross);
                }
            }
            if (((thing != null) && !thing.Medical) && (thing.owner != actor))
            {
                if (actor.Downed)
                {
                    actor.Position = GenCellFinder.RandomStandableClosewalkCellNear(actor.Position, 1);
                }
                actor.jobs.EndCurrentJob(JobCondition.Incompletable);
            }
            else if (this.pawn.IsHashIntervalTick(0xd3) && this.ShouldGetUpNow(this.pawn, thing))
            {
                actor.jobs.CheckForJobOverride();
            }
        }

        public static bool MayGetUpVoluntarily(Pawn pawn)
        {
            return (!pawn.health.ShouldDoSurgeryNow && !pawn.health.ShouldGetTreatment);
        }

        private bool ShouldGetUpNow(Pawn pawn, Building_Bed bed)
        {
            if (!pawn.Downed)
            {
                if ((pawn is TameablePawn) && TimetablePreventsLayDown(pawn))
                {
                    return true;
                }
                if (!this.asleepInt)
                {
                    if (bed == null)
                    {
                        return true;
                    }
                    if (((pawn.needs.food.CurCategory >= HungerCategory.UrgentlyHungry) && !pawn.CurJob.playerForced) && MayGetUpVoluntarily(pawn))
                    {
                        return true;
                    }
                    if (bed.Medical)
                    {
                        if (!pawn.health.CanUseMedicalBed)
                        {
                            return true;
                        }
                        if ((base.CurJob.getOutOfMedBedIfTreated && !pawn.health.ShouldGetTreatment) && !pawn.health.ShouldDoSurgeryNow)
                        {
                            return true;
                        }
                    }
                    if (!pawn.health.WantsToGoToBedForMedicine && !pawn.health.WantsToRemainInBedForMedicine)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool TimetablePreventsLayDown(Pawn pawn)
        {
            return (((((pawn.timetable != null) && !pawn.timetable.CurrentAssignment.allowRest) && (pawn.needs.rest.CurLevel >= 0.2f)) && ((pawn.CurJob == null) || !pawn.CurJob.playerForced)) && MayGetUpVoluntarily(pawn));
        }

        // Properties
        public bool Asleep
        {
            get
            {
                return this.asleepInt;
            }
        }

        public override PawnPosture Posture
        {
            get
            {
                return (!this.layingDownInt ? PawnPosture.Standing : PawnPosture.LayingAny);
            }
        }
    }
}