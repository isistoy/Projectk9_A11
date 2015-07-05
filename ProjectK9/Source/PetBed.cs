using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;
using RimWorld;
using ProjectK9.AI;

namespace ProjectK9
{
    public class PetBed : Building_Bed
    {
        // This is a total hack. The PetBedHolder is just a pawn to assign as the owner of PetBeds so that Colonists
        //  don't try to sleep in this bed when looking for a bed to sleep in. Pets have to be wary when dealing
        //  with bed ownership that they have to assign the owner of the PetBed to null before trying to claim it.
        public static TameablePawn PetBedHolder = new TameablePawn()
        {
            thingIDNumber = Find.World.nextThingId,
            def = ThingDef.Named("Mutt")
        };

        public override IEnumerable<Gizmo> GetGizmos()
        {
            // We don't want to have pets as "prisoners". That makes me a sad panda.
            //  Otherwise this bed is identical to the Colonist's "Building_Bed" with a new Texture.

            yield break;
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            if (owner == PetBedHolder)
                owner = null;
            base.Destroy(mode);
        }

        public TameablePawn CurPetOccupant
        {
            get
            {
                List<Thing> list = Find.ThingGrid.ThingsListAt(base.Position);
                for (int i = 0; i < list.Count; i++)
                {
                    TameablePawn pet = list[i] as TameablePawn;
                    if (((pet != null) && (pet.jobs.curJob != null)) 
                        && ((pet.jobs.curJob.def == JobDefOf.LayDown)
                        && (pet.jobs.curJob.targetA.Thing == this)))
                    {
                        return pet;
                    }
                }
                return null;
            }
        }

        public override void DrawGUIOverlay()
        {
            if (((Find.CameraMap.CurrentZoom == CameraZoomRange.Closest) && (((this.owner == null) || !this.owner.InBed()) || (this.owner.CurrentBed().owner != this.owner))))
            {
                string nickname;
                if (this.owner != null)
                {
                    nickname = this.owner != PetBedHolder ? this.owner.Nickname : "Unowned".Translate();
                }
                else
                {
                    nickname = "Unowned".Translate();
                }
                GenWorldUI.DrawThingLabel(this, nickname, new Color(1f, 1f, 1f, 0.75f));
            }
        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(Translator.Translate("Owner") + ": ");
            if (this.owner == null || this.owner == PetBedHolder)
                stringBuilder.Append(Translator.Translate("Nobody"));
            else
                stringBuilder.Append(this.owner.Label);

            return stringBuilder.ToString();
        }
        public override void SpawnSetup()
        {
            base.SpawnSetup();
            if (this.owner == null)
                this.owner = PetBedHolder;
        }
        public override void ExposeData()
        {
            base.ExposeData();
            //Scribe_References.LookReference<TameablePawn>(ref PetBedHolder, "petBedHolder");
        }
    }
}