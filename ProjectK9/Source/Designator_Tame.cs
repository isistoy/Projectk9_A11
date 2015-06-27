using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using UnityEngine;

namespace ProjectK9
{
    public class Designator_Tame : Designator
    {
        private List<Pawn> justTamed = new List<Pawn>();

        public Designator_Tame()
        {
            base.defaultLabel = "Tame";
            base.defaultDesc = "Taming wild animal";
            base.icon = ContentFinder<Texture2D>.Get("UI/Commands/Tame", true);
            base.soundDragSustain = SoundDefOf.DesignateDragStandard;
            base.soundDragChanged = SoundDefOf.DesignateDragStandardChanged;
            base.useMouseIcon = true;
            base.soundSucceeded = SoundDefOf.DesignateHunt;
            base.hotKey = KeyBindingDefOf.Misc11;
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 c)
        {
            if (!c.InBounds())
            {
                return AcceptanceReport.WasRejected;
            }
            if (!this.TameableInCell(c).Any<Pawn>())
            {
                return "You must designate tameable animals";
            }
            return AcceptanceReport.WasAccepted;
        }

        public override AcceptanceReport CanDesignateThing(Thing t)
        {
            Pawn pawn = t as Pawn;
            if (((pawn != null) && !pawn.def.race.Humanlike) && (pawn is TameablePawn && (Find.DesignationManager.DesignationOn(pawn, TamePawnUtility.GetTameDesDef()) == null)))
            {
                return AcceptanceReport.WasAccepted;
            }
            return AcceptanceReport.WasRejected;
        }

        public override void DesignateSingleCell(IntVec3 loc)
        {
            IEnumerator<Pawn> enumerator = this.TameableInCell(loc).GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    Pawn current = enumerator.Current;
                    this.DesignateThing(current);
                }
            }
            finally
            {
                if (enumerator == null)
                {
                }
                enumerator.Dispose();
            }
        }

        public override void DesignateThing(Thing t)
        {
            Find.DesignationManager.AddDesignation(new Designation(t, TamePawnUtility.GetTameDesDef()));
            this.justTamed.Add((Pawn)t);
        }

        protected override void FinalizeDesignationSucceeded()
        {
            base.FinalizeDesignationSucceeded();
            IEnumerator<PawnKindDef> enumerator = this.justTamed.Select<Pawn, PawnKindDef>(p => p.kindDef).Distinct<PawnKindDef>().GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    PawnKindDef current = enumerator.Current;
                    if (current.manhunterOnDamageChance > 0.5f)
                    {
                        object[] args = new object[] { current.label };
                        Messages.Message("MessageAnimalsGoPsychoHunted".Translate(args), MessageSound.Standard);
                    }
                }
            }
            finally
            {
                if (enumerator == null)
                {
                }
                enumerator.Dispose();
            }
            this.justTamed.Clear();
        }

        private IEnumerable<Pawn> TameableInCell(IntVec3 c)
        {
            List<Thing> thingList;
            int i;
            if (!c.Fogged())
            {
                thingList = c.GetThingList();
                i = 0;
                while (i < thingList.Count)
                {
                    if (this.CanDesignateThing(thingList[i]).Accepted)
                    {
                        yield return (Pawn)thingList[i];
                    }
                    i++;
                }
            }
        }

        public override int DraggableDimensions
        {
            get
            {
                return 2;
            }
        }
    }

}
