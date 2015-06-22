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

        private bool isColonyPet = false;

        public bool IsColonyPet
        {
            get
            {
                return isColonyPet;
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
            if (!IsColonyPet || this.RaceProps.bodySize >= 0.5f)
            {
                return null;
            }
            Thought_Observation obs;
            obs = (Thought_Observation)ThoughtMaker.MakeThought(ThoughtDef.Named("SawCuteDog"));
            obs.Target = this;
            return obs;
        }

        public void InitializeColonyPet()
        {
            try
            {
                SetFactionDirect(getPetFaction());
                isColonyPet = true;
                initBasicPet();
                generateStory();
                SetFactionDirect(Faction.OfColony);
                initWorkSettings();
                if (guest == null)
                {
                    Log.Message("guest");
                    guest = new Pawn_GuestTracker(this);
                    //Log.Message("Setting guest Status");
                    //this.guest.SetGuestStatus(Faction.OfColony);
                }
                //Log.Message("Hack: forcing faction OfColony for fooling reservations/ensuring hauling!");
                //SetFactionDirect(Faction.OfColony);
            }
            catch (Exception ex)
            {
                Log.ErrorOnce(string.Concat("Message: ", ex.Message), 900900900);
                Log.ErrorOnce(string.Concat("Stack: ", ex.StackTrace), 900900901);
            }
        }

        private void initWorkSettings()
        {
            //if (playerController == null)
            //{
            //    Log.Message("player controller");
            //    playerController = new Pawn_PlayerController(this);
            //}
            if (workSettings == null)
            {
                Log.Message("worksettings");
                workSettings = new Pawn_WorkSettings(this);
            }
            if (jobs == null)
            {
                Log.Message("jobs");
                jobs = new Pawn_JobTracker(this);
            }
            else if (jobs.curJob != null)
                jobs.EndCurrentJob(JobCondition.Incompletable);

            workSettings.EnableAndInitialize();
            workSettings.DisableAll();
            List<WorkTypeDef> workTypes = new List<WorkTypeDef>();
            if (this.def.defName == "Mutt")
            {
                if (!story.WorkTypeIsDisabled(WorkTypeDefOf.Hauling))
                    workTypes.Add(WorkTypeDefOf.Hauling);
            }
            else if (this.def.defName == "Shep")
            {
                if (!story.WorkTypeIsDisabled(WorkTypeDefOf.Hunting))
                    workTypes.Add(WorkTypeDefOf.Hunting);
                if (!story.WorkTypeIsDisabled(WorkTypeDefOf.Hauling))
                    workTypes.Add(WorkTypeDefOf.Hauling);
            }
            Log.Message("Trying to add new work types");
            foreach (WorkTypeDef workType in workTypes)
                workSettings.SetPriority(workType, 4);
        }

        private void initBasicPet()
        {
            if (ownership == null)
            {
                Log.Message("ownership");
                ownership = new Pawn_Ownership(this);
            }
            if (pather == null)
            {
                Log.Message("pather");
                pather = new Pawn_PathFollower(this);
            }
            else
            {
                pather.ResetToCurrentPosition();
            }
            if (natives == null)
            {
                Log.Message("natives");
                natives = new Pawn_NativeVerbs(this);
            }
            if (thinker == null)
            {
                Log.Message("thinker");
                thinker = new Pawn_Thinker(this);
            }
           
            //if (needs.mood.thoughts == null)
            //{
            //    Log.Message("thoughts");
            //    needs.mood.thoughts = new ThoughtHandler(this);
            //}
            if (mindState == null)
            {
                Log.Message("mindstate");
                mindState = new Pawn_MindState(this);
            }
            if (filth == null)
            {
                Log.Message("filth");
                filth = new Pawn_FilthTracker(this);
            }
            if (stances == null)
            {
                Log.Message("stances");
                stances = new Pawn_StanceTracker(this);
            }
            if (carryHands == null)
            {
                Log.Message("carryhands");
                carryHands = new Pawn_CarryHands(this);
            }
            if (inventory == null)
            {
                Log.Message("inventory");
                inventory = new Pawn_InventoryTracker(this);
            }
            if (equipment == null)
            {
                equipment = new Pawn_EquipmentTracker(this);
            }
            if (apparel == null)
            {
                Log.Message("custom apparel");
                apparel = new PetApparelOverride(this);
            }
            if (needs == null)
            {
                Log.Message("needs");
                needs = new Pawn_NeedsTracker(this);
            }
            if (needs.mood == null)
            {
                Log.Message("mood");
                typeof(Pawn_NeedsTracker).GetMethod("AddNeed", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(this.needs, new object[] { DefDatabase<NeedDef>.GetNamed("Mood", true) });
            }
        }

        private void generateStory()
        {
             if (skills == null)
            {
                Log.Message("skills");
                skills = new Pawn_SkillTracker(this);
                //story.GenerateSkillsFromBackstory();
            }
            if (story == null)
            {
                Log.Message("story");
                story = new Pawn_StoryTracker(this);
            }
            
            // TO DO : randomize access to backstories, by their spawnCategories
            Backstory childStory = BackstoryDatabase.allBackstories.Where(stor => 
                stor.Value.spawnCategories.Exists( cat => cat == "pet") && stor.Value.slot == BackstorySlot.Childhood)
                .RandomElement().Value;
            Backstory adultStory = BackstoryDatabase.allBackstories.Where(stor =>
                stor.Value.spawnCategories.Exists(cat => cat == "pet") && stor.Value.slot == BackstorySlot.Adulthood)
                .RandomElement().Value;
            story.childhood = childStory;
                //BackstoryDatabase.GetWithKey(BackstoryDefExt.UniqueSaveKeyFor(BackstoryDef.Named("k9_calm_childhood")));
            story.adulthood = adultStory;
                //BackstoryDatabase.GetWithKey(BackstoryDefExt.UniqueSaveKeyFor(BackstoryDef.Named("k9_playful_adulthood")));            

            if (story.traits == null)
            {
                Log.Message("traits");
                story.traits = new TraitSet(this);
            }

            Log.Message("name");
            story.name = Name = PawnNameMaker.GenerateName(this);
            story.name.ResolveMissingPieces();
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

        private Faction getPetFaction()
        {
            FactionDef petFactionDef = FactionDef.Named("ColonyPets");
            Faction petFaction = Find.FactionManager.FirstFactionOfDef(petFactionDef);

            if (petFaction == null)
            {
                // This should probably never happen.
                Log.Warning("Creating the Pet Faction for the First time.");
                petFaction = new Faction();
                petFaction.def = petFactionDef;
                petFaction.name = "ColonyPets";
                Find.FactionManager.Add(petFaction);
            }

            return petFaction;
        }

        public override void ExposeData()
        {
            //if (this.jobs != null && this.jobs.curDriver != null)
            //{
            //    this.jobs.EndCurrentJob(JobCondition.Errored);
            //}
            base.ExposeData();
            Scribe_Values.LookValue<bool>(ref isColonyPet, "IsColonyPet");
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
            if (isColonyPet)
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
