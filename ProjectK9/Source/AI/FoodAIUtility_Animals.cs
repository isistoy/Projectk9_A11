using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;

namespace ProjectK9.AI
{
    public static class FoodAIUtility_Animals
    {
        public static JobDef GetEatMeatJobDef()
        {
            return DefDatabase<JobDef>.GetNamed("EatForAnimals");
        }

        public static JobDef GetEatCorpseJobDef()
        {
            return DefDatabase<JobDef>.GetNamed("EatCorpse");
        }

        public static JobDef GetHuntForAnimalsJobDef()
        {
            return DefDatabase<JobDef>.GetNamed("HuntForAnimals");
        }

        public static IEnumerable<Thing> ButcherCorpseProducts(Corpse corpse, Pawn butcher)
        {
            if (corpse.def.butcherProducts != null)
            {
                IEnumerator<Thing> butchEnumerator = corpse.innerPawn.ButcherProducts(butcher, 1f).GetEnumerator();
                try
                {
                    while (butchEnumerator.MoveNext())
                    {
                        yield return butchEnumerator.Current;
                    }
                }
                finally
                {
                    butchEnumerator.Dispose();
                }            
            }
            else
                Log.Message(string.Concat("No inventory things found in corpse ", corpse, " for pawn ", butcher));
            
            if (corpse.innerPawn.RaceProps.isFlesh)
            {
                FilthMaker.MakeFilth(butcher.Position, ThingDefOf.FilthBlood, corpse.innerPawn.LabelCap);
            }

            int meatCount = Mathf.RoundToInt(corpse.innerPawn.def.race.MeatAmount);
            if (meatCount > 0)
            {
                Thing meat = ThingMaker.MakeThing(corpse.innerPawn.def.race.meatDef, null);

                if (meat != null)
                {
                    meat.stackCount = meatCount;
                    yield return meat;
                }                
            }

            if (corpse.innerPawn.def.race.hasLeather && (corpse.innerPawn.def.race.leatherDef != null))
            {
                int LeatherCount = Mathf.RoundToInt(corpse.innerPawn.def.race.LeatherAmount);
                if (LeatherCount > 0)
                {
                    Thing leather = ThingMaker.MakeThing(corpse.innerPawn.def.race.leatherDef, null);
                    if (leather != null)
                    {
                        leather.stackCount = LeatherCount;
                        yield return leather;
                    }
                }
            }
        }

        public static void Ingested(Thing ingested, Pawn ingester, float nutritionWanted)
        {
            if (!ingested.IngestibleNow)
            {
                Log.Error(ingester + " ingested IngestibleNow=false thing " + ingested);
            }
            else
            {
                //if (ingester.needs.mood != null)
                //{
                //    if (ingested.def.ingestible.ingestedDirectThought != null)
                //    {
                //        ingester.needs.mood.thoughts.TryGainThought(ingested.def.ingestible.ingestedDirectThought);
                //    }
                //}
                //float poisonPercent = 0f;
                //if ((ingested.def.ingestible.preferability == AIFoodPreferability.Raw) && !ingester.RaceProps.Animal)
                //{
                //    poisonPercent = 0.02f;
                //}
                //Meal meal = ingested as Meal;
                //if (meal != null)
                //{
                //    poisonPercent = meal.PoisonPercent;
                //}
                //if (Rand.Value < poisonPercent)
                //{
                //    ingester.health.AddHediff(HediffMaker.MakeHediff(HediffDefOf.FoodPoisoning, ingester), null, null);
                //}
                int count = Mathf.CeilToInt(nutritionWanted / ingested.def.ingestible.nutrition);
                int[] values = new int[] { count, ingested.def.ingestible.maxNumToIngestAtOnce, ingested.stackCount };
                count = Mathf.Max(Mathf.Min(values), 1);
                if (count >= ingested.stackCount)
                {
                    count = ingested.stackCount;
                    ingested.Destroy(DestroyMode.Vanish);
                }
                else
                {
                    ingested.SplitOff(count);
                }
                ingester.needs.food.CurLevel += count * ingested.def.ingestible.nutrition;
                if (ingester.needs.joy != null)
                {
                    JoyKindDef joyKind = (ingested.def.ingestible.joyKind == null) ? JoyKindDefOf.Gluttonous : ingested.def.ingestible.joyKind;
                    ingester.needs.joy.GainJoy(count * ingested.def.ingestible.joy, joyKind);
                }
                ingested.def.ingestible.Worker.IngestedBy(ingester, ingested, count);
            }
        }

        public static Toil FinalizeEatForAnimals(Pawn ingester, TargetIndex ingestibleInd)
        {
            Toil ingest = new Toil() { defaultCompleteMode = ToilCompleteMode.Instant };
            ingest.initAction = new Action(() =>
                {
                    Thing ingested = ingest.actor.jobs.curJob.GetTarget(ingestibleInd).Thing;
                    float nutritionWanted = 1f - ingester.needs.food.CurLevel;
                    Ingested(ingested, ingester, nutritionWanted);
                });
            return ingest;
        }
    }
}

