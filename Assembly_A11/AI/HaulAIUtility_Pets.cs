using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using Verse.AI;

namespace ProjectK9.AI
{
    // Rewritten only for reserving things as if standard Colonist Faction 
    public static class HaulAIUtility_Pets
    {
        private static string NoEmptyPlaceLowerTrans = "NoEmptyPlaceLower".Translate();

        public static Job HaulToStorageJob(TameablePawn p, Thing t)
        {
            IntVec3 vec;

            StoragePriority currentPriority = StoragePriorityAtFor(t.Position, t);
            // HACK here for faction
            Log.Message("before Hack");
            if (!StoreUtility.TryFindBestBetterStoreCellFor(t, p, currentPriority, Faction.OfColony, out vec, true))
            {
                JobFailReason.Is(NoEmptyPlaceLowerTrans);
                return null;
            }
            return HaulMaxNumToCellJob(p, t, vec, false);
        }

        private static StoragePriority StoragePriorityAtFor(IntVec3 c, Thing t)
        {
            if (t.SpawnedInWorld)
            {
                SlotGroup group = Find.SlotGroupManager.SlotGroupAt(c);
                if ((group != null) && group.Settings.AllowedToAccept(t))
                {
                    return group.Settings.Priority;
                }
            }
            return StoragePriority.Unstored;
        }

        private static Job HaulMaxNumToCellJob(Pawn p, Thing t, IntVec3 storeCell, bool fitInStoreCell)
        {
            Job job = new Job(DefDatabase<JobDef>.GetNamed("HaulForPets"), t, storeCell);
            if (Find.SlotGroupManager.SlotGroupAt(storeCell) != null)
            {
                Thing thing = Find.ThingGrid.ThingAt(storeCell, t.def);
                if (thing != null)
                {
                    job.maxNumToCarry = t.def.stackLimit;
                    if (fitInStoreCell)
                    {
                        job.maxNumToCarry -= thing.stackCount;
                    }
                }
                else
                {
                    job.maxNumToCarry = 0x1869f;
                }
            }
            else
            {
                job.maxNumToCarry = 0x1869f;
            }
            job.haulOpportunisticDuplicates = true;
            job.haulMode = HaulMode.ToCellStorage;
            return job;
        }
    }
}
