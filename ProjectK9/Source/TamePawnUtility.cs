using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using Verse.AI;
using System.Reflection;
using RimWorld.SquadAI;

namespace ProjectK9
{
    public static class TamePawnUtility
    {
        public static Faction GetPetFaction()
        {
            Faction petFaction = Find.FactionManager.FirstFactionOfDef(GetPetFactionDef());
            if (petFaction == null)
                Log.Error("The pet faction ColonyPets is not created and missing.");
            return petFaction;
        }

        public static FactionDef GetPetFactionDef()
        {
            return FactionDef.Named("ColonyPets");
        }

        public static bool CanTame(Pawn pawn, TameablePawn tamee)
        {
            IntVec3 vec = pawn.Position - tamee.Position;
            if (vec.LengthHorizontalSquared > 36f)
            {
                return false;
            }
            if (!GenSight.LineOfSight(pawn.Position, tamee.Position, true))
            {
                return false;
            }
            return true;
        }

        public static bool TryTame(TameMessage msg, Pawn pawn, TameablePawn tamee)
        {
            if (!CanTame(pawn, tamee))
                return false;
            if ((msg.ThoughtDef != null) && (tamee.needs.mood != null))
            {
                float num = pawn.GetStatValue(StatDefOf.SocialImpact, true);
                if (num > 0.01f)
                {
                    Thought_Memory newThought = (Thought_Memory)ThoughtMaker.MakeThought(msg.ThoughtDef);
                    newThought.powerFactor = num;
                    tamee.needs.mood.thoughts.TryGainThought(newThought);
                }
            }
            if (msg.Effect == TameEffect.TryTame)
            {
                TryTameColonyPet(pawn, tamee);
            }
            MoteThrower.MakeSpeechOverlay(pawn);
            return true;
        }

        public static void TryTameColonyPet(Pawn pawn, TameablePawn tamee)
        {
            try
            {
                if (tamee != null)
                {
                    Log.Message("Initializing new Pet");
                    //tamee.SetFactionDirect(GetPetFaction());
                    //initBasicPet(tamee);
                    //tamee.pather.StopDead();
                    //tamee.jobs.StopAll();
                    //tamee.mindState.Reset();
                    //Find.ListerPawns.UpdateRegistryForPawn(tamee);
                    Find.ListerPawns.DeRegisterPawn(tamee);
                    Find.PawnDestinationManager.RemovePawnFromSystem(tamee);
                    Find.Reservations.ReleaseAllClaimedBy(tamee);
                    Brain squadBrain = tamee.GetSquadBrain();
                    if (squadBrain != null)
                    {
                        squadBrain.Notify_PawnLost(tamee, PawnLostCondition.ChangedFaction);
                    }
                    ((Thing)tamee).SetFaction(GetPetFaction());
                    //GenerateStory(tamee);
                    InitWorkSettings(tamee);
                    Reachability.ClearCache();
                    tamee.health.surgeryBills.Clear();
                    Find.ListerPawns.RegisterPawn(tamee);
                    Find.GameEnder.CheckGameOver();
                    tamee.IsColonyPet = true;
                    Designation tameDes = Find.DesignationManager.DesignationOn(tamee, DefDatabase<DesignationDef>.GetNamed("Tame"));
                    if (tameDes != null)
                        Find.DesignationManager.RemoveDesignation(tameDes);
                    Log.Message("Removed Taming designation");
                }

            }
            catch (Exception ex)
            {
                Log.ErrorOnce(string.Concat("Message: ", ex.Message), 900900900);
                Log.ErrorOnce(string.Concat("Stack: ", ex.StackTrace), 900900901);
            }
        }

        public static void InitWorkSettings(TameablePawn tamee)
        {
            if (tamee.workSettings == null)
            {
                Log.Message("worksettings");
                tamee.workSettings = new Pawn_WorkSettings(tamee);
            }
            if (tamee.jobs == null)
            {
                Log.Message("jobs");
                tamee.jobs = new Pawn_JobTracker(tamee);
            }

            tamee.workSettings.EnableAndInitialize();
            tamee.workSettings.DisableAll();
            List<WorkTypeDef> workTypes = new List<WorkTypeDef>();
            if (tamee.def.defName == "Mutt")
            {
                if (!tamee.story.WorkTypeIsDisabled(WorkTypeDefOf.Hauling))
                    workTypes.Add(WorkTypeDefOf.Hauling);
            }
            else if (tamee.def.defName == "Shep")
            {
                if (!tamee.story.WorkTypeIsDisabled(WorkTypeDefOf.Hunting))
                    workTypes.Add(WorkTypeDefOf.Hunting);
                if (!tamee.story.WorkTypeIsDisabled(WorkTypeDefOf.Hauling))
                    workTypes.Add(WorkTypeDefOf.Hauling);
            }
            Log.Message("Trying to add new work types");
            foreach (WorkTypeDef workType in workTypes)
                tamee.workSettings.SetPriority(workType, 4);
        }

        public static void InitBasicPet(TameablePawn tamee)
        {
            if (tamee.pather == null)
            {
                Log.Message("pather");
                tamee.pather = new Pawn_PathFollower(tamee);
            }
            //if (tamee.playerController == null)
            //{
            //    Log.Message("player controller");
            //    tamee.playerController = new Pawn_PlayerController(tamee);
            //}
            if (tamee.ownership == null)
            {
                Log.Message("ownership");
                tamee.ownership = new Pawn_Ownership(tamee);
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
                Log.Message("equipment");
                tamee.equipment = new Pawn_EquipmentTracker(tamee);
            }
            if (tamee.apparel == null)
            {
                Log.Message("apparel");
                tamee.apparel = new Pawn_ApparelTracker(tamee);
                //tamee.apparel = new Tameable_ApparelTracker(tamee);
            }
            if (tamee.skills == null)
            {
                Log.Message("skills");
                tamee.skills = new Pawn_SkillTracker(tamee);
            }
            if (tamee.story == null)
            {
                Log.Message("story");
                tamee.story = new Pawn_StoryTracker(tamee);
            }
            if (tamee.story.traits == null)
            {
                Log.Message("traits");
                tamee.story.traits = new TraitSet(tamee);
            }
            if (tamee.mindState == null)
            {
                Log.Message("mindstate");
                tamee.mindState = new Pawn_MindState(tamee);
            }
            if ((tamee.needs != null) && (tamee.needs.mood == null))
            {
                Log.Message("mood");
                typeof(Pawn_NeedsTracker)
                    .GetMethod("AddNeed", BindingFlags.NonPublic | BindingFlags.Instance)
                    .Invoke(tamee.needs, new object[] { DefDatabase<NeedDef>.GetNamed("Mood", true) });
            }
        }

        public static void GenerateStory(TameablePawn tamee)
        {
            // randomize backstories, filtering on their spawnCategories
            Backstory childStory = BackstoryDatabase.allBackstories.Where(stor =>
                stor.Value.spawnCategories.Exists(cat => cat == "pet") && stor.Value.slot == BackstorySlot.Childhood)
                .RandomElement().Value;
            Backstory adultStory = BackstoryDatabase.allBackstories.Where(stor =>
                stor.Value.spawnCategories.Exists(cat => cat == "pet") && stor.Value.slot == BackstorySlot.Adulthood)
                .RandomElement().Value;
            tamee.story.childhood = childStory;
            tamee.story.adulthood = adultStory;
            Log.Message("name");
            tamee.story.name = tamee.Name = generatePetName();
            tamee.story.name.ResolveMissingPieces();
        }

        private static PawnName generatePetName()
        {
            IEnumerable<string> extantNames = ListerPawnNames.AllPawnsNamesEverUsed.Select<PawnName, string>(pn => pn.ToString());
            return PawnName.FromString(GetPetFactionDef().pawnNameMaker.GenerateDefault_Name(extantNames));
        }
    }

    public struct TameMessage
    {
        private ThoughtDef thoughtDef;
        private TameEffect effect;
        public TameMessage(ThoughtDef thoughtDef)
        {
            this.thoughtDef = thoughtDef;
            this.effect = TameEffect.None;
        }
        public TameMessage(ThoughtDef thoughtDef, TameEffect effect)
            : this(thoughtDef)
        {
            this.effect = effect;
        }
        public ThoughtDef ThoughtDef
        {
            get
            {
                return thoughtDef;
            }
        }
        public TameEffect Effect
        {
            get
            {
                return effect;
            }
        }
    }

    public enum TameEffect : byte
    {
        None = 0,
        TryTame = 1
    }

    public enum TameableInteractionMode : byte
    {
        NoInteraction = 0,
        Pet = 1,
        AttemptTame = 2
    }
}
