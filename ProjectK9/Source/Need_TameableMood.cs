
using System;
using System.Collections.Generic;
using System.Text;

using RimWorld;
using UnityEngine;
using Verse;

namespace ProjectK9
{
    public class Need_TameableMood : Need_Mood
    {
        public Need_TameableMood(Pawn pawn)
            : base(pawn)
        {
            //this.thoughts = new ThoughtHandler(pawn);
            //this.observer = new PawnObserver(pawn);
            //this.recentMemory = new PawnRecentMemory(pawn);
        }

        //public override void DrawOnGUI(Rect rect)
        //{
        //    if (base.threshPercents == null)
        //    {
        //        base.threshPercents = new List<float>();
        //    }
        //    base.threshPercents.Clear();
        //    base.threshPercents.Add(base.pawn.mindState.breaker.HardBreakThreshold);
        //    base.threshPercents.Add(base.pawn.mindState.breaker.SoftBreakThreshold);
        //    base.DrawOnGUI(rect);
        //}

        //public override void ExposeData()
        //{
        //    base.ExposeData();
        //    object[] ctorArgs = new object[] { base.pawn };
        //    Scribe_Deep.LookDeep<ThoughtHandler>(ref this.thoughts, "thoughts", ctorArgs);
        //    object[] objArray2 = new object[] { base.pawn };
        //    Scribe_Deep.LookDeep<PawnRecentMemory>(ref this.recentMemory, "recentMemory", objArray2);
        //}

        //public override string GetTipString()
        //{
        //    StringBuilder builder = new StringBuilder();
        //    builder.AppendLine(base.GetTipString());
        //    builder.AppendLine();
        //    builder.AppendLine("MentalBreakThresholdHard".Translate() + ": " + base.pawn.mindState.breaker.HardBreakThreshold.ToStringPercent());
        //    builder.AppendLine("MentalBreakThresholdSoft".Translate() + ": " + base.pawn.mindState.breaker.SoftBreakThreshold.ToStringPercent());
        //    return builder.ToString();
        //}

        public override void NeedInterval()
        {
            //base.NeedInterval();
            //this.recentMemory.RecentMemoryInterval();
            //this.thoughts.ThoughtInterval();
            //this.observer.ObserverInterval();
        }

        public override float CurInstantLevel
        {
            get
            {
                //float num = this.thoughts.TotalMood();
                //if (base.pawn.IsColonist || base.pawn.IsPrisonerOfColony)
                //{
                //    num += Find.Storyteller.difficulty.colonistMoodOffset;
                //}
                //return Mathf.Clamp01(base.def.baseLevel + (num / 100f));
                return 1f;
            }
        }
       
    }
}
