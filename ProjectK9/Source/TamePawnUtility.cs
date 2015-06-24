using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using Verse.AI;
using System.Reflection;

namespace ProjectK9
{
    public static class TamePawnUtility
    {
        public static Faction GetPetFaction()
        {
            FactionDef petFactionDef = FactionDef.Named("ColonyPets");
            Faction petFaction = Find.FactionManager.FirstFactionOfDef(petFactionDef);

            if (petFaction == null)
                Log.Error("The pet faction ColonyPets is not created and missing.");
            return petFaction;
        }
        
        public static void InitializeColonyPet(TameablePawn tamee)
        {
            try
            {
                tamee.SetFactionDirect(GetPetFaction());
                tamee.IsColonyPet = true;
                initBasicPet(tamee);
                generateStory(tamee);
                initWorkSettings(tamee);

                tamee.pather.StopDead();
                tamee.jobs.StopAll();
                Find.Reservations.ReleaseAllClaimedBy(tamee);
                tamee.mindState.Reset();
                Find.ListerPawns.UpdateRegistryForPawn(tamee);

            }
            catch (Exception ex)
            {
                Log.ErrorOnce(string.Concat("Message: ", ex.Message), 900900900);
                Log.ErrorOnce(string.Concat("Stack: ", ex.StackTrace), 900900901);
            }
        }

        private static void initWorkSettings(TameablePawn tamee)
        {
            if (tamee.workSettings == null)
            {
                Log.Message("worksettings");
                workSettings = new Pawn_WorkSettings(this);
            }
            if (jobs == null)
            {
                Log.Message("jobs");
                jobs = new Pawn_JobTracker(this);
            }

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

        private static void initBasicPet(TameablePawn tamee)
        {
            if (tamee.ownership == null)
            {
                Log.Message("ownership");
                tamee.ownership = new Pawn_Ownership(tamee);
            }
            if (tamee.pather == null)
            {
                Log.Message("pather");
                tamee.pather = new Pawn_PathFollower(tamee);
            }
            if (tamee.playerController == null)
            {
                Log.Message("player controller");
                tamee.playerController = new Pawn_PlayerController(tamee);
            }
            if (tamee.guest == null)
            {
                Log.Message("guest");
                tamee.guest = new Pawn_GuestTracker(tamee);
            }
            if (tamee.natives == null)
            {
                Log.Message("natives");
                tamee.natives = new Pawn_NativeVerbs(tamee);
            }
            if (tamee.thinker == null)
            {
                Log.Message("thinker");
                tamee.thinker = new Pawn_Thinker(tamee);
            }

            //if (needs.mood.thoughts == null)
            //{
            //    Log.Message("thoughts");
            //    needs.mood.thoughts = new ThoughtHandler(this);
            //}
            if (tamee.mindState == null)
            {
                Log.Message("mindstate");
                tamee.mindState = new Pawn_MindState(tamee);
            }
            if (tamee.filth == null)
            {
                Log.Message("filth");
                tamee.filth = new Pawn_FilthTracker(tamee);
            }
            if (tamee.stances == null)
            {
                Log.Message("stances");
                tamee.stances = new Pawn_StanceTracker(tamee);
            }
            if (tamee.carryHands == null)
            {
                Log.Message("carryhands");
                tamee.carryHands = new Pawn_CarryHands(tamee);
            }
            if (tamee.inventory == null)
            {
                Log.Message("inventory");
                tamee.inventory = new Pawn_InventoryTracker(tamee);
            }
            if (tamee.equipment == null)
            {
                tamee.equipment = new Pawn_EquipmentTracker(tamee);
            }
            if (tamee.apparel == null)
            {
                Log.Message("custom apparel");
                tamee.apparel = new PetApparelOverride(tamee);
            }
            if (tamee.needs == null)
            {
                Log.Message("needs");
                tamee.needs = new Pawn_NeedsTracker(tamee);
            }
            if (tamee.needs.mood == null)
            {
                Log.Message("mood");
                typeof(Pawn_NeedsTracker).GetMethod("AddNeed", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(tamee.needs, new object[] { DefDatabase<NeedDef>.GetNamed("Mood", true) });
            }
        }

        private static void generateStory(TameablePawn tamee)
        {
            if (tamee.skills == null)
            {
                Log.Message("skills");
                tamee.skills = new Pawn_SkillTracker(tamee);
                //story.GenerateSkillsFromBackstory();
            }
            if (tamee.story == null)
            {
                Log.Message("story");
                tamee.story = new Pawn_StoryTracker(tamee);
            }

            // TO DO : randomize access to backstories, by their spawnCategories
            Backstory childStory = BackstoryDatabase.allBackstories.Where(stor =>
                stor.Value.spawnCategories.Exists(cat => cat == "pet") && stor.Value.slot == BackstorySlot.Childhood)
                .RandomElement().Value;
            Backstory adultStory = BackstoryDatabase.allBackstories.Where(stor =>
                stor.Value.spawnCategories.Exists(cat => cat == "pet") && stor.Value.slot == BackstorySlot.Adulthood)
                .RandomElement().Value;
            tamee.story.childhood = childStory;
            //BackstoryDatabase.GetWithKey(BackstoryDefExt.UniqueSaveKeyFor(BackstoryDef.Named("k9_calm_childhood")));
            tamee.story.adulthood = adultStory;
            //BackstoryDatabase.GetWithKey(BackstoryDefExt.UniqueSaveKeyFor(BackstoryDef.Named("k9_playful_adulthood")));            

            if (tamee.story.traits == null)
            {
                Log.Message("traits");
                tamee.story.traits = new TraitSet(tamee);
            }

            Log.Message("name");
            tamee.story.name = tamee.Name = PawnNameMaker.GenerateName(tamee);
            tamee.story.name.ResolveMissingPieces();
        }
    }
}
