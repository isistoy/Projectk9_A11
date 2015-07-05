using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;
using Verse.AI;
using RimWorld;

namespace ProjectK9.AI
{
    public class JobDriver_TakeToBedAnimal : JobDriver
    {
        // Fields
        private const TargetIndex BedIndex = TargetIndex.B;
        private const TargetIndex TakeeIndex = TargetIndex.A;

        // Methods
        public JobDriver_TakeToBedAnimal()
        {
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOn<JobDriver_TakeToBedAnimal>(() =>
                {
                    if (this.CurJob.def.makeTargetPrisoner)
                    {
                        if (this.DropBed.ForPrisoners)
                        {
                            return true;
                        }
                    }
                    else if (this.DropBed.ForPrisoners != ((Pawn)((Thing)this.TargetA)).IsPrisoner)
                    {
                        return true;
                    }
                    return false;
                });
            // Animal reservation (Target A)
            yield return Toils_Reserve.Reserve(TargetIndex.A, 1);
            // Bed reservation (Target B)
            yield return Toils_Reserve.Reserve(TargetIndex.B, 1);
            // Claiming Bed, if necessary
            yield return Toils_Bed.ClaimBedIfNonMedical(TargetIndex.B, TargetIndex.A);
            // Going to animal
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch)
                .FailOnDespawnedOrForbidden<Toil>(TargetIndex.A)
                .FailOnDespawnedOrForbidden<Toil>(TargetIndex.B)
                .FailOn<Toil>(() =>
                {
                    return ((this.CurJob.def == JobDefOf.Arrest) && !this.Takee.CanBeArrested());

                })
                .FailOn<Toil>(() =>
                {
                    return !this.pawn.CanReach(this.DropBed, PathEndMode.OnCell, Danger.Deadly);

                });
            // Checking if will try to resist arrest (of animal use?)
            Toil checkArrestResist = new Toil();
            checkArrestResist.initAction = new Action(() =>
            {
                if ((this.CurJob.def == JobDefOf.Arrest)
                    && !((Pawn)this.CurJob.targetA.Thing).CheckAcceptArrest(this.pawn))
                {
                    this.pawn.jobs.EndCurrentJob(JobCondition.Incompletable);
                }
            });
            yield return checkArrestResist;
            // Making animal uncounscious, for autoheal or eventual medical operation
            Toil makeUnconscious = new Toil();
            makeUnconscious.initAction = new Action(() =>
            {
                if (this.CurJob.applyAnesthetic)
                {
                    HealthUtility.TryAnesthesize(this.Takee);
                }
            });
            yield return makeUnconscious;
            // Carry animal to bed
            yield return Toils_Haul.StartCarryThing(TargetIndex.A);
            // Go to bed
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch).FailOnDestroyed<Toil>(TargetIndex.A);
            // Make animal a guest or prisoner (of animal use?)
            Toil makeGuestOrPrisoner = new Toil();
            makeGuestOrPrisoner.initAction = new Action(() =>
            {
                if (this.CurJob.def.makeTargetPrisoner)
                {
                    if (!this.Takee.IsPrisonerOfColony)
                    {
                        if (this.Takee.Faction != null)
                        {
                            this.Takee.Faction.Notify_MemberCaptured(this.Takee, this.pawn.Faction);
                        }
                        this.Takee.guest.SetGuestStatus(Faction.OfColony, true);
                        if (this.Takee.guest.IsPrisoner)
                        {
                            object[] args = new object[] { this.pawn, this.Takee };
                            TaleRecorder.RecordTale(TaleDef.Named("Captured"), args);
                        }
                    }
                }
                else if ((this.Takee.Faction != Faction.OfColony) && (this.Takee.HostFaction != Faction.OfColony))
                {
                    this.Takee.guest.SetGuestStatus(Faction.OfColony, false);
                }
            });
            yield return makeGuestOrPrisoner;
            // Release bed reservation for animal laydown/sleep
            yield return Toils_Reserve.Release(TargetIndex.B);
            // As the name says :)
            Toil tuckInBed = new Toil();
            tuckInBed.initAction = new Action(() =>
            {
                Thing thing;
                IntVec3 position = this.DropBed.Position;
                this.pawn.carryHands.TryDropCarriedThing(position, ThingPlaceMode.Direct, out thing);
                if (!this.DropBed.Destroyed && (this.DropBed.owner == this.Takee))
                {
                    RestAIUtility_Animal.Notify_TuckedIntoBedAnimal(Takee as TameablePawn, this.DropBed);
                }
                if (this.Takee.IsPrisonerOfColony)
                {
                    ConceptDecider.TeachOpportunity(ConceptDefOf.PrisonerTab, this.Takee, OpportunityType.GoodToKnow);
                }
            });
            tuckInBed.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return tuckInBed;
        }

        // Properties
        protected Building_Bed DropBed
        {
            get
            {
                return (Building_Bed)base.CurJob.targetB.Thing;
            }
        }
        protected Pawn Takee
        {
            get
            {
                return (Pawn)base.CurJob.targetA.Thing;
            }
        }

    }
}
