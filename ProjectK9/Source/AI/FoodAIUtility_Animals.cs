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

        private static IEnumerable<Thing> ButcherCorpseProducts(this Corpse corpse)
        {
            if (corpse.def.butcherProducts == null)
                yield break;
            int i = 0;
            if (i < corpse.def.butcherProducts.Count)
            {
                ThingCount counter = corpse.def.butcherProducts[i];
                Thing result = ThingMaker.MakeThing(counter.thingDef, null);
                result.stackCount = counter.count;
                yield return result;
                i++;
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
            Toil ingest = new Toil() { defaultCompleteMode = ToilCompleteMode.Instant};
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

