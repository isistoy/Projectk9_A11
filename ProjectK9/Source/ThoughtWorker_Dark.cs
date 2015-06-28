using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace ProjectK9
{
    public class ThoughtWorker_Dark : RimWorld.ThoughtWorker_Dark
    {
        public ThoughtWorker_Dark()
        { }

        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            TameablePawn tamee = p as TameablePawn;
            if (tamee != null)
            {
                return false;
            }
            return base.CurrentStateInternal(p);
        }
    }
}
