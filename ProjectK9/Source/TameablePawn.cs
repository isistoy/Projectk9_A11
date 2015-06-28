using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Verse;
using Verse.AI;
using UnityEngine;
using RimWorld;
using Backstories;
using System.Reflection;

namespace ProjectK9
{
    public class TameablePawn : Pawn, IThoughtGiver
    {
        private static Color pawnNameColor = new Color(0.9f, 0.9f, 0.9f);

        public TameablePawn_TameTracker tameTracker;

        public bool IsColonyPet
        {
            get
            {
                if (tameTracker != null)
                    return tameTracker.IsTamed;
                return false;
            }
        }

        public override string LabelBase
        {
            get
            {
                if (IsColonyPet)
                    return (string.Concat(
                        this.Nickname,
                        ", ",
                        ageTracker.AgeNumberString,
                        ", Colony Pet")
                        );
                else
                    return string.Concat("pet", this.thingIDNumber.ToString());
            }
        }

        public override string LabelBaseShort
        {
            get
            {
                if (IsColonyPet)
                    return this.Nickname;
                else
                    return string.Concat("pet", this.thingIDNumber.ToString());
            }
        }

        public override void DrawGUIOverlay()
        {
            if (IsColonyPet)
                drawPetOverlay();
            else
                base.DrawGUIOverlay();
        }

        private void drawPetOverlay()
        {
            if ((this.SpawnedInWorld && !Find.FogGrid.IsFogged(this.Position)))
            {
                Vector3 vector = (Vector3)GenWorldUI.LabelDrawPosFor(this, -0.6f);
                float y = vector.y;

                Text.Font = GameFont.Tiny;
                float x = GUI.skin.label.CalcSize(new GUIContent(this.LabelBaseShort)).x;
                if (x < 20f)
                    x = 20f;
                Rect position = new Rect((vector.x - (x / 2f)) - 4f, vector.y, x + 8f, 12f);
                GUI.DrawTexture(position, TexUI.GrayTextBG);
                if (this.health.summaryHealth.SummaryHealthPercent < 0.999f)
                    Widgets.FillableBar(position.ContractedBy(1f), this.health.summaryHealth.SummaryHealthPercent, PawnUIOverlay.HealthTex, BaseContent.ClearTex, false);
                GUI.color = pawnNameColor;
                Text.Anchor = TextAnchor.UpperCenter;
                Widgets.Label(new Rect(vector.x - (x / 2f), vector.y - 2f, x, 999f), this.LabelBaseShort);
                GUI.color = Color.white;
                Text.Anchor = TextAnchor.UpperLeft;
                y += 12f;
            }
        }

        public override void PostMake()
        {            
            base.PostMake();
            if (!IsColonyPet)
            {
                TamePawnUtility.InitBasicPet(this);
                TamePawnUtility.GenerateStory(this);
            }
        }

        //public override void PostMapInit()
        //{

        //    base.PostMapInit();
        //}

        //private void CleanupToughts()
        //{
        //    /// Fixed bad thoughts reference that we want out
        //    /// Would have to be loaded from a custom def directly
        //    List<ThoughtDef> forbidThoughtDefs = new List<ThoughtDef>();
        //    forbidThoughtDefs.Add(ThoughtDef.Named("Naked"));
        //    forbidThoughtDefs.Add(ThoughtDef.Named("ApparelDamaged"));
        //    forbidThoughtDefs.Add(ThoughtDefOf.AteWithoutTable);
        //    forbidThoughtDefs.Add(ThoughtDef.Named("AteRawFood"));
        //    forbidThoughtDefs.Add(ThoughtDefOf.AteHumanlikeMeatDirect);
        //    forbidThoughtDefs.Add(ThoughtDefOf.AteHumanlikeMeatAsIngredient);
        //    forbidThoughtDefs.Add(ThoughtDefOf.ObservedLayingCorpse);
        //    forbidThoughtDefs.Add(ThoughtDefOf.ObservedLayingRottingCorpse);
        //    forbidThoughtDefs.Add(ThoughtDef.Named("SharedBedroom"));

        //    var query =
        //        from c in needs.mood.thoughts.DistinctThoughtDefs
        //        join f in forbidThoughts on c.defName equals f.defName
        //        select new { c };
        //}

        public override IEnumerable<FloatMenuOption> GetExtraFloatMenuOptionsFor(IntVec3 sq)
        {
            yield return new FloatMenuOption("testaction", new Action(() => { }));
        }

        public override void DeSpawn()
        {
            if (ownership != null && ownership.ownedBed != null)
            {
                ownership.ownedBed.owner = PetBed.PetBedHolder;
                ownership = null;
            }
            base.DeSpawn();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            if (!IsColonyPet)
            {
                Command_Toggle tame = new Command_Toggle();
                tame.defaultLabel = "Tame";
                tame.defaultDesc = "Tame this pet";
                tame.icon = ContentFinder<Texture2D>.Get("UI/Commands/Tame");
                tame.isActive = IsDesignatedToBeTamed;
                if (IsDesignatedToBeTamed())
                    tame.toggleAction = removePawnTamingDesignation;
                else
                    tame.toggleAction = addPawnTamingDesignation;

                tame.hotKey = KeyBindingDefOf.CommandItemForbid;
                yield return tame;
            }
            yield break;
        }

        public Thought GiveObservedThought()
        {
            int num1 = 10;
            int num2 = UnityEngine.Random.Range(1, 100);
            if (!IsColonyPet || (num2 < num1))
            {
                return null;
            }

            Thought_Observation obs = (Thought_Observation)ThoughtMaker.MakeThought(ThoughtDef.Named("SawCuteDog"));
            obs.Target = this;
            return obs;
        }

        //public override void SetFaction(Faction newFaction)
        //{
        //    if (newFaction == base.Faction)
        //    {
        //        Log.Warning(string.Concat(new object[] { "Used ChangePawnFactionTo to change ", this, " to same faction ", newFaction }));
        //    }
        //    else
        //    {
        //        //Log.Message("deregistering pawn");
        //        //Find.ListerPawns.DeRegisterPawn(this);
        //        //Find.PawnDestinationManager.RemovePawnFromSystem(this);

        //        if (health == null)
        //        {
        //            Log.Message("health");
        //            health = new Pawn_HealthTracker(this);
        //        }

        //        if (ageTracker == null)
        //        {
        //            Log.Message("age");
        //            ageTracker = new Pawn_AgeTracker(this);
        //        }
        //        if (pather == null)
        //        {
        //            Log.Message("path");
        //            pather = new Pawn_PathFollower(this);
        //        }

        //        if (needs == null)
        //        {
        //            Log.Message("needs");
        //            needs = new Pawn_NeedsTracker(this);
        //        }
        //        if (stances == null)
        //        {
        //            Log.Message("stances");
        //            stances = new Pawn_StanceTracker(this);
        //        }
        //        if (thinker == null)
        //        {
        //            Log.Message("thinker");
        //            thinker = new Pawn_Thinker(this);
        //        }
        //        if (inventory == null)
        //        {
        //            Log.Message("inventory");
        //            inventory = new Pawn_InventoryTracker(this);
        //        }
        //        if (playerController == null)
        //        {
        //            Log.Message("playercontroller");
        //            playerController = new Pawn_PlayerController(this);
        //        }
        //        if (caller == null)
        //        {
        //            Log.Message("caller");
        //            caller = new Pawn_CallTracker(this);
        //        }

        //        Log.Message("direct faction");
        //        SetFactionDirect(newFaction);

        //        //if (base.Faction.def == FactionDefOf.Colony)
        //        //{
        //        //    this.workSettings.EnableAndInitialize();
        //        //}
        //        //else if ((this.playerController != null) && this.playerController.Drafted)
        //        //if ((this.playerController != null) && this.playerController.Drafted)
        //        //{
        //        //    this.playerController.Drafted = false;
        //        //}
        //        //Log.Message("Cleaning up reach and components");
        //        //Reachability.ClearCache();
        //        //PawnUtility.AddAndRemoveComponentsAsAppropriate(this);
        //        //this.health.surgeryBills.Clear();
        //        //Find.ListerPawns.RegisterPawn(this);
        //        //Find.GameEnder.CheckGameOver();
        //    }

        //}        

        public override void ExposeData()
        {
            //if (this.jobs != null && this.jobs.curDriver != null)
            //{
            //    this.jobs.EndCurrentJob(JobCondition.Errored);
            //}
            base.ExposeData();
            object[] objArray1 = new object[] { this };
            Scribe_Deep.LookDeep<TameablePawn_TameTracker>(ref this.tameTracker, "tameTracker", objArray1);            
        }

        public bool IsDesignatedToBeTamed()
        {
            return getTamingDesignationOnSelf() != null;
        }

        private void removePawnTamingDesignation()
        {
            Designation des = getTamingDesignationOnSelf();
            if (des != null)
                Find.DesignationManager.RemoveDesignation(des);
        }

        private Designation getTamingDesignationOnSelf()
        {
            return Find.DesignationManager.DesignationOn(this, DefDatabase<DesignationDef>.GetNamed("Tame"));
        }

        private void addPawnTamingDesignation()
        {
            Find.DesignationManager.AddDesignation(new Designation(this, DefDatabase<DesignationDef>.GetNamed("Tame")));
        }

        private WorkTypeDef getDef(string defName)
        {
            return DefDatabase<WorkTypeDef>.GetNamed(defName);
        }

        public override string ToString()
        {
            if (IsColonyPet)
            {
                return LabelBaseShort;
            }
            else
                return string.Concat("pet", this.thingIDNumber.ToString());
        }

        //protected override void ApplyDamage(DamageInfo dinfo)
        //{
        //    Log.Message(string.Concat(this, " taking ", dinfo.Amount, " damage. New health will be: ", (this.health - dinfo.Amount)));
        //    base.ApplyDamage(dinfo);
        //}

        //public override void Killed(DamageInfo dam)
        //{
        //    Log.Message(string.Concat("Killing: ", this));
        //    base.Killed(dam);
        //    Log.Message(string.Concat(this, " has been killed"));
        //}

        //public override void Destroy()
        //{
        //    Log.Message(string.Concat("Destroying: ", this));
        //    base.Destroy();
        //    Log.Message(string.Concat(this, " has been destroyed"));
        //}
    }
}