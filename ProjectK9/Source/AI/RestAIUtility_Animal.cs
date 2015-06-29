using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace ProjectK9.AI
{
    public static class RestAIUtility_Animal
    {
        public static JobDef GetSleepJobDef()
        {
            return DefDatabase<JobDef>.GetNamed("SleepForAnimals");
        }
    }
}
