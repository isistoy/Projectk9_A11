using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;
using Verse.AI;
using RimWorld;

namespace ProjectK9.AI
{
    public class JobGiver_GetRestForAnimal : ThinkNode_JobGiver
    {
        public override float GetPriority(Pawn pawn)
        {
            if (pawn.needs.rest.CurLevel < 0.50f)
            {
                return 8f;
            }
            if (!pawn.needs.food.Starving && pawn.health.hediffSet.GetNaturallyHealingInjuredParts().Any<BodyPartRecord>())
            {
                return 3f;
            }
            return 0f;
        }

        protected override Job TryGiveTerminalJob(Pawn pawn)
        {
            JobDef restJobDef = JobDefOf.LayDown;
            TameablePawn pet = pawn as TameablePawn;
            if ((pet.jobs.curJob == null) || ((pet.jobs.curJob.def != restJobDef) && pet.Awake()))
            {
                if ((!pawn.Downed) && (!pawn.Broken))
                {
                    //Log.Message(string.Concat(pawn, " trying to sleep"));
                    if (pet.IsColonyPet)
                    {
                        Building_Bed bedFor = pawn.ownership.ownedBed;

                        if (bedFor == null)
                            bedFor = RestAIUtility_Animal.FindUnownedBed(pawn);
                        
                        if (bedFor != null)
                            return new Job(restJobDef, bedFor);                            
                    }
                    return new Job(restJobDef, GenCellFinder.RandomStandableClosewalkCellNear(pawn.Position, 4));
                }
            }
            return null;
        }
    }
}
