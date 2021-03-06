﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using Verse.AI;
using System.Reflection;
using RimWorld.SquadAI;
using UnityEngine;

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

        public static DesignationDef GetTameDesDef()
        {
            return DefDatabase<DesignationDef>.GetNamed("Tame");
        }

        public static JobDef GetTameJobDef()
        {
            return DefDatabase<JobDef>.GetNamed("Tame");
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
            MoteThrower.MakeSpeechOverlay(pawn);
            if (msg.Effect == TameEffect.TryTame)
            {
                float f = pawn.GetStatValue(StatDefOf.RecruitPrisonerChance, true);
                float tameDifficulty = tamee.tameTracker.TameDifficulty;
                if (tamee.needs.mood != null)
                {
                    if (tamee.needs.mood.CurLevel < 0.35f)
                    {
                        object[] args = new object[] { pawn.Nickname, tamee, 0.35f.ToStringPercent() };
                        Messages.Message(string.Concat(args[0], " couldn't tame ", args[1], " because mood is below ", args[2]), MessageSound.Silent);
                        return false;
                    } 
                }
                // Added value for balance
                f *= 1f - (tameDifficulty / 100f);
                if (f < 0.011f)
                {
                    f = 0.01f;
                }

                if (Rand.Range(0f, 1f) > f)
                {
                    object[] args = new object[] { pawn.Nickname, tamee, f.ToStringPercent() };
                    Messages.Message(string.Concat(args[0], " couldn't tame ", args[1], ". It failed with a chance of ", args[2]), MessageSound.Standard);
                    return false;
                }
                tamee.tameTracker.DoTamePet();
                Messages.Message(string.Concat(tamee, " has been tamed by ", pawn.Nickname), MessageSound.Benefit);
            }
            return true;
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
            Log.Message("intializing Work");
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
            {
                tamee.workSettings.SetPriority(workType, 4);
            }
        }

        public static void InitBasicPet(TameablePawn tamee)
        {
            if (tamee.health == null)
            {
                Log.Message("health");
                tamee.health = new Pawn_HealthTracker(tamee);
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
            if (tamee.mindState == null)
            {
                Log.Message("mindstate");
                tamee.mindState = new Pawn_MindState(tamee);
            }
            if (tamee.tameTracker == null)
            {
                Log.Message("tametracker");
                tamee.tameTracker = new TameablePawn_TameTracker(tamee);
                tamee.tameTracker.Init();
            }
        }

        private static NeedDef GetTameableMoodDef()
        {
            return new NeedDef
            {
                defName = "TameableMood",
                needClass = typeof(Need_TameableMood),
                label = "Living being mood",
                description = "Mood represents how happy or stressed a living begin is. If mood gets too low, the living being may behave in unpredictive ways.",
                showOnNeedList = false,
                minIntelligence = Intelligence.ToolUser,
                baseLevel = 0.5f,
                seekerRisePerHour = 0f,
                seekerFallPerHour = 0f,
                listPriority = 0,
                major = false,
                freezeWhileSleeping = true
            };
        }

        public static void CreateTameableMood(TameablePawn tamee)
        {
            if (tamee.needs == null)
            {
                Log.Message("needs");
                tamee.needs = new Pawn_NeedsTracker(tamee);
            }
            if ((tamee.needs != null) && (tamee.needs.mood == null))
            {
                Log.Message("mood");
                //typeof(Pawn_NeedsTracker)
                //    .GetMethod("AddNeed", BindingFlags.NonPublic | BindingFlags.Instance)
                //    .Invoke(tamee.needs, new object[] { TamePawnUtility.GetTameableMoodDef() });

                typeof(Pawn_NeedsTracker).
                    GetMethod("AddNeed", BindingFlags.NonPublic | BindingFlags.Instance).
                    Invoke(tamee.needs, new object[] { DefDatabase<NeedDef>.GetNamed("Mood", true) });
            }
        }

        public static void GenerateStory(TameablePawn tamee)
        {
            // randomize backstories, filtering on their spawnCategories
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
            Backstory childStory = BackstoryDatabase.allBackstories.Where(stor =>
                stor.Value.spawnCategories.Exists(cat => cat == "pet") && stor.Value.slot == BackstorySlot.Childhood)
                .RandomElement().Value;
            Backstory adultStory = BackstoryDatabase.allBackstories.Where(stor =>
                stor.Value.spawnCategories.Exists(cat => cat == "pet") && stor.Value.slot == BackstorySlot.Adulthood)
                .RandomElement().Value;
            tamee.story.childhood = childStory;
            tamee.story.adulthood = adultStory;
            Log.Message("adding resilient trait");
            tamee.story.traits.GainTrait(new Trait(DefDatabase<TraitDef>.GetNamed("Resilient")));            
            tamee.story.name = tamee.Name = generatePetName();
            tamee.story.name.ResolveMissingPieces();
            Log.Message(string.Concat("name is ", tamee.Name));
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
}
