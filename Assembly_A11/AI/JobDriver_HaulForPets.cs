using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;
using Verse.AI;
using RimWorld;

namespace ProjectK9.AI
{
    public class JobDriver_HaulForPets : JobDriver
    {
        private const TargetIndex HaulableInd = TargetIndex.A;
        private const TargetIndex StoreCellInd = TargetIndex.B;

        public JobDriver_HaulForPets()
        {
        }

        public override string GetReport()
        {
            IntVec3 cell = base.pawn.jobs.curJob.targetB.Cell;
            Thing carriedThing = null;
            if (base.pawn.carryHands.CarriedThing != null)
            {
                carriedThing = base.pawn.carryHands.CarriedThing;
            }
            else
            {
                carriedThing = base.TargetThingA;
            }
            string str = null;
            SlotGroup slotGroup = cell.GetSlotGroup();
            if (slotGroup != null)
            {
                str = slotGroup.parent.SlotYielderLabel();
            }
            if (str != null)
            {
                object[] objArray1 = new object[] { carriedThing.LabelCap, str };
                return "ReportHaulingTo".Translate(objArray1);
            }
            object[] args = new object[] { carriedThing.LabelCap };
            return "ReportHauling".Translate(args);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            // HACK
            pawn.SetFactionDirect(Faction.OfColony);

            this.FailOnDestroyed<JobDriver_HaulForPets>(TargetIndex.A);
            this.FailOnBurningImmobile<JobDriver_HaulForPets>(TargetIndex.B);
            if (!TargetThingA.IsForbidden(pawn))
            {
                this.FailOnForbidden<JobDriver_HaulForPets>(TargetIndex.A);
            }

            yield return Toils_Reserve.Reserve(TargetIndex.B, 1);
            
            Toil reserveTargetA = null;
            reserveTargetA = Toils_Reserve.Reserve(TargetIndex.A, 1);
            yield return reserveTargetA;

            Toil petGoto = null;
            petGoto = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch);
            petGoto.FailOn<Toil>(checkStorage);
            yield return petGoto;

            yield return Toils_Haul.StartCarryThing(TargetIndex.A);
            if (!CurJob.haulOpportunisticDuplicates)
            {
                yield break;
            }
            yield return Toils_Haul.CheckForGetOpportunityDuplicate(reserveTargetA, TargetIndex.A, TargetIndex.B);

            Toil carryToCell = Toils_Haul.CarryHauledThingToCell(TargetIndex.B);
            yield return carryToCell;
            Toil placeToCell = Toils_Haul.PlaceHauledThingInCell(TargetIndex.B, carryToCell,true);
            placeToCell.AddFinishAction(resetFaction);
            yield return placeToCell;
        }

        private bool checkStorage()
        {
            Pawn actor = this.GetActor();
            Job curJob = actor.jobs.curJob;
            if (curJob.haulMode == HaulMode.ToCellStorage)
            {
                Thing storable = curJob.GetTarget(TargetIndex.A).Thing;
                if (!actor.jobs.curJob.GetTarget(TargetIndex.B).Cell.IsValidStorageFor(storable))
                {
                    return true;
                }
            }
            return false;
        }

        private void resetFaction()
        {
            pawn.SetFactionDirect(HerdUtility.GetColonyPetFaction());
        }
    }
}
