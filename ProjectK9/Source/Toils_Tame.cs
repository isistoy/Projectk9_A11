using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using Verse.AI;

namespace ProjectK9
{
    public static class Toils_Tameable
    {
        public static Toil ConvinceTameable(Pawn pawn, TameablePawn tamee)
        {
            return new Toil
            {
                initAction = new Action(() =>
                    { 
                        /// tame logic here
                        if (!TamePawnUtility.TryTame(new TameMessage(ThoughtDef.Named("FriendIsCaring"), TameEffect.None), pawn, tamee))
                        {
                            pawn.jobs.curDriver.ReadyForNextToil();
                        }
                        else
                        {
                            pawn.skills.Learn(SkillDefOf.Social, 30f);
                        }
                    }),
                defaultCompleteMode = ToilCompleteMode.Delay,
                defaultDuration = 350
            };
        }

        public static Toil GotoTameable(Pawn pawn, TameablePawn tamee, TameableInteractionMode mode)
        {
            Toil toil = new Toil
            {
               initAction = new Action(() => pawn.pather.StartPath((Pawn)tamee, PathEndMode.Touch))
            };
            toil.AddFailCondition(() =>
                {
                    if (!tamee.Destroyed)
                    {
                        if (!tamee.Awake())
                            return true;
                        if ((mode == TameableInteractionMode.Pet) || (mode == TameableInteractionMode.AttemptTame))
                            return false;
                    }
                    return true;
                });
            toil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
            return toil;
        }
    }
}
