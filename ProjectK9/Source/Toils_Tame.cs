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

        public static Toil GotoTameable(Pawn pawn, TameablePawn tamee)
        {
            Toil toil = new Toil
            {
                initAction = new Action(() => 
                    {
                        pawn.pather.StartPath((Pawn)tamee, PathEndMode.Touch);
                    })
            };
            toil.AddFailCondition(() =>
                {
                    if (!tamee.Destroyed)
                    {
                        if (!tamee.Awake())
                            return true;
                    }
                    return false;
                });
            toil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
            return toil;
        }

        public static Toil SetTameableLastInteractTime(TargetIndex ind)
        {
            Toil toil = new Toil();
            toil.initAction = () =>
                {
                    Log.Message(string.Concat("Set last visit time by ",toil.actor));
                    TameablePawn thing = (TameablePawn)toil.actor.CurJob.GetTarget(ind).Thing;
                    if (thing.tameTracker != null)
                    {
                        Log.Message(string.Concat("Setting tamervisittime ", Find.TickManager.TicksGame, " to ", thing));
                        thing.tameTracker.lastTamerVisitTime = Find.TickManager.TicksGame;
                    }
                    else
                    {
                        Log.Message(string.Concat("Can't set last tamer visit time to ", thing));
                        toil.actor.jobs.StopAll();
                    }
                };

            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            return toil;
        }

        public static Toil TryTameTameable(TargetIndex ind)
        {
            Toil toil = new Toil();
            toil.initAction = () =>
                {
                    Pawn actor = toil.actor;
                    TameablePawn thing = (TameablePawn)actor.jobs.curJob.GetTarget(ind).Thing;
                    if ( !thing.Destroyed && thing.Awake() )
                    {
                        TamePawnUtility.TryTame(new TameMessage(null, TameEffect.TryTame), actor, thing);
                    }
                };
            toil.defaultCompleteMode = ToilCompleteMode.Delay;
            toil.defaultDuration = 100;
            return toil;
        }
    }
}
