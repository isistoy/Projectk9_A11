using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;
using RimWorld;
using UnityEngine;
using RimWorld.SquadAI;

namespace ProjectK9
{
    public class TameablePawn_TameTracker : IExposable
    {
        private Faction hostFactionInt;
        public bool isTamed;
        public int lastTamerVisitTime;
        private const float MaxTameDifficulty = 99f;
        public int MinInteractionInterval;
        private const float MinTameDifficulty = 30f;
        private TameablePawn tameable;
        private float tameDifficulty;

        public Faction HostFaction
        {
            get { return this.hostFactionInt; }
        }

        public bool IsTamed
        {
            get { return this.isTamed; }
        }

        public bool TameeIsSecure
        {
            get
            {
                if (this.tameable.HostFaction == null)
                    return false;
                if (this.tameable.BrokenStateDef != null)
                    return false;
                if ((this.tameable.jobs.curJob != null) && this.tameable.jobs.curJob.exitMapOnArrival)
                    return false;
                return true;
            }
        }

        public bool TameeScheduledForInteraction
        {
            get
            {
                Log.Message(
                    string.Concat("Last tame interaction time is ", lastTamerVisitTime, 
                    ". Checking at ", Find.TickManager.TicksGame, 
                    ". Could start at ", lastTamerVisitTime + this.MinInteractionInterval));
                return (this.lastTamerVisitTime < (Find.TickManager.TicksGame - this.MinInteractionInterval));
            }
        }

        public float TameDifficulty
        {
            get
            {
                return this.tameDifficulty;
            }
        }

        public TameablePawn_TameTracker()
        {
            this.lastTamerVisitTime = -9999;
            this.tameDifficulty = -1f;
            this.MinInteractionInterval = 7500;
        }

        public TameablePawn_TameTracker(TameablePawn tameable)
        {
            this.lastTamerVisitTime = -9999;
            this.tameDifficulty = -1f;
            this.MinInteractionInterval = 7500;
            this.tameable = tameable;
        }

        public void ExposeData()
        {
            Scribe_References.LookReference<Faction>(ref this.hostFactionInt, "hostFaction");
            Scribe_Values.LookValue<bool>(ref this.isTamed, "pet", false, false);
            Scribe_Values.LookValue<int>(ref this.lastTamerVisitTime, "lastTamerVisitTime", -9999, false);
            Scribe_Values.LookValue<float>(ref this.tameDifficulty, "tameDifficulty", 0f, false);
        }

        public void Init()
        {
            if (this.tameDifficulty < 0f)
            {
                Log.Message("baseRecruitDifficulty");
                float baseDifficulty = this.tameable.kindDef.baseRecruitDifficulty + Rand.Range((float)-30f, (float)30f);
                //float popIntent = (Game.Mode != GameMode.MapPlaying) ? 1f : Find.Storyteller.intenderPopulation.PopulationIntent;
                //baseDifficulty = PopIntentAdjustedTameDifficulty(baseDifficulty, popIntent);
                Log.Message("recruitDifficulty");
                this.tameDifficulty = Mathf.Clamp(baseDifficulty, 30f, 99f);
            }
        }

        public static float PopIntentAdjustedTameDifficulty(float baseDifficulty, float popIntent)
        {
            float num = Mathf.Clamp(popIntent, 0.25f, 3f);
            float num2 = 100f - ((100f - baseDifficulty) * num);
            return Mathf.Clamp(num2, 30f, 99f);
        }
        //public static DoTables_PopIntentTameDifficulty();
        //public void SetTamedStatus(Faction newHost);

        public void DoTamePet()
        {
            try
            {
                if ((this.tameable != null) && !this.IsTamed)
                {
                    tameable.SetFaction(Faction.OfColony);
                    this.isTamed = true;
                    Log.Message(string.Concat("new pet tamed ", this.tameable));
                    Designation tameDes = Find.DesignationManager.DesignationOn(this.tameable, DefDatabase<DesignationDef>.GetNamed("Tame"));
                    if (tameDes != null)
                        Find.DesignationManager.RemoveDesignation(tameDes);
                }
            }
            catch (Exception ex)
            {
                Log.ErrorOnce(string.Concat("Message: ", ex.Message), 900900900);
                Log.ErrorOnce(string.Concat("Stack: ", ex.StackTrace), 900900901);
            }
        }

    }
}
