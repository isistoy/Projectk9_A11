using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using System.Runtime.InteropServices;
using Verse.AI;
using UnityEngine;

namespace ProjectK9
{
    public static class TameablePawnGenerator
    {
        public const int maxMentalBreakThreshold = 40;

        public static TameablePawn GenerateTameable(PawnKindDef kindDef, Faction faction, [Optional, DefaultParameterValue(0)] int tries)
        {
            //if ((kindDef.race.thingClass == typeof(TameablePawn)) && (faction == null))
            //{
            //    faction = FactionUtility.DefaultFactionFrom(kindDef.defaultFactionType);
            //    Log.Error(string.Concat(new object[] { "Tried to generate pawn of Humanlike race ", kindDef, " with null faction. Setting to ", faction }));
            //}

            TameablePawn newPawn = (TameablePawn)ThingMaker.MakeThing(kindDef.race, null);
            newPawn.kindDef = kindDef;
            newPawn.SetFactionDirect(faction);
            TamePawnUtility.InitBasicPet(newPawn);
            if (newPawn.RaceProps.hasGenders)
            {
                if (Rand.Value < 0.5f)
                {
                    newPawn.gender = Gender.Male;
                }
                else
                {
                    newPawn.gender = Gender.Female;
                }
            }
            else
            {
                newPawn.gender = Gender.None;
            }
            TamePawnUtility.GenerateStory(newPawn);
            GenerateRandomAge(newPawn);
            TamePawnUtility.InitWorkSettings(newPawn);
            TamePawnUtility.CreateTameableMood(newPawn);
            //newPawn.pather = new Pawn_PathFollower(newPawn);
            //newPawn.ageTracker = new Pawn_AgeTracker(newPawn);
            //newPawn.health = new Pawn_HealthTracker(newPawn);
            //newPawn.jobs = new Pawn_JobTracker(newPawn);
            //newPawn.mindState = new Pawn_MindState(newPawn);
            //newPawn.filth = new Pawn_FilthTracker(newPawn);
            //newPawn.needs = new Pawn_NeedsTracker(newPawn);
            //newPawn.stances = new Pawn_StanceTracker(newPawn);
            //newPawn.InitUnsavedUniversalComponents();
            //if (newPawn.RaceProps.ToolUser)
            //{
            //    newPawn.equipment = new Pawn_EquipmentTracker(newPawn);
            //    newPawn.carryHands = new Pawn_CarryHands(newPawn);
            //    newPawn.apparel = new Pawn_ApparelTracker(newPawn);
            //    newPawn.inventory = new Pawn_InventoryTracker(newPawn);
            //}
            //if (newPawn.RaceProps.Humanlike)
            //{
            //    newPawn.ownership = new Pawn_Ownership(newPawn);
            //    newPawn.skills = new Pawn_SkillTracker(newPawn);
            //    newPawn.talker = new Pawn_TalkTracker(newPawn);
            //    newPawn.story = new Pawn_StoryTracker(newPawn);
            //    newPawn.workSettings = new Pawn_WorkSettings(newPawn);
            //}
            //if (newPawn.RaceProps.intelligence <= Intelligence.ToolUser)
            //{
            //    newPawn.caller = new Pawn_CallTracker(newPawn);
            //}
            
            //GenerateInitialHediffs(newPawn);
            //if (newPawn.RaceProps.Humanlike)
            //{
            //    newPawn.story.skinColor = PawnSkinColors.RandomSkinColor();
            //    newPawn.story.crownType = (Rand.Value >= 0.5f) ? CrownType.Narrow : CrownType.Average;
            //    newPawn.story.headGraphicPath = GraphicDatabaseHeadRecords.GetHeadRandom(newPawn.gender, newPawn.story.skinColor, newPawn.story.crownType).GraphicPath;
            //    newPawn.story.hairColor = PawnHairColors.RandomHairColor(newPawn.story.skinColor, newPawn.ageTracker.AgeBiologicalYears);
            //    PawnBioGenerator.GiveAppropriateBioTo(newPawn, faction.def);
            //    newPawn.story.hairDef = PawnHairChooser.RandomHairDefFor(newPawn, faction.def);
            //    GiveRandomTraitsTo(newPawn);
            //    newPawn.story.GenerateSkillsFromBackstory();
            //    if (faction.def == FactionDefOf.Colony)
            //    {
            //        newPawn.workSettings.EnableAndInitialize();
            //    }
            //}
            //PawnApparelGenerator.GenerateStartingApparelFor(newPawn);
            //PawnWeaponGenerator.TryGenerateWeaponFor(newPawn);
            //PawnInventoryGenerator.GenerateInventoryFor(newPawn);
            //PawnUtility.AddAndRemoveComponentsAsAppropriate(newPawn);
            if (!newPawn.Dead)
            {
                return newPawn;
            }
            if (tries < 10)
            {
                return GenerateTameable(kindDef, faction, tries + 1);
            }
            Log.Error("Generated dead pawn " + newPawn.ThingID + ". Too many tries, returning null.");
            return null;
        }

        private static void GenerateRandomAge(TameablePawn pawn)
        {
            int num8;
            int num2 = 0;
            int num3 = 0;
            Log.Message(string.Concat("Generating age for ", pawn));
            if (pawn.ageTracker == null)
            {
                pawn.ageTracker = new Pawn_AgeTracker(pawn);
            }
        Label_0004:
            if (pawn.RaceProps.ageGenerationCurve != null)
            {
                num2 = Mathf.RoundToInt(Rand.ByCurve(pawn.RaceProps.ageGenerationCurve, 200));
            }
            else if (pawn.RaceProps.mechanoid)
            {
                num2 = Rand.Range(0, 2500);
            }
            else if (pawn.RaceProps.Animal)
            {
                num2 = Rand.Range(1, 10);
            }
            else
            {
                Log.Warning("Didn't get age for " + pawn);
                return;
            }
            num3++;
            if (num3 > 100)
            {
                Log.Error("Tried 100 times to generate age for " + pawn);
            }
            else if ((num2 > pawn.kindDef.maxGenerationAge) || (num2 < pawn.kindDef.minGenerationAge))
            {
                goto Label_0004;
            }

            pawn.ageTracker.AgeBiologicalTicks = (num2 * 3600000L) + Rand.Range(0, 3600000);

            int num4 = (Game.Mode != GameMode.MapPlaying) ? (int)MapInitData.startingMonth : Find.TickManager.TicksAbs;
            long absTicks = num4 - pawn.ageTracker.AgeBiologicalTicks;
            int birthYear = GenDate.CalendarYearAt(absTicks);
            int birthDayOfYear = GenDate.DayOfYearAt(absTicks);
            if (Rand.Value < pawn.kindDef.backstoryCryptosleepCommonality)
            {
                float num9 = UnityEngine.Random.value;
                if (num9 < 0.7f)
                {
                    num8 = UnityEngine.Random.Range(0, 100);
                }
                else if (num9 < 0.95f)
                {
                    num8 = UnityEngine.Random.Range(100, 1000);
                }
                else
                {
                    int max = (GenDate.CurrentYear - 2026) - pawn.ageTracker.AgeBiologicalYears;
                    num8 = UnityEngine.Random.Range(1000, max);
                }
            }
            else
            {
                num8 = 0;
            }
            birthYear -= num8;
            pawn.ageTracker.SetChronologicalBirthDate(birthYear, birthDayOfYear);

        }
    }
}
