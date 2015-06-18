using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;
using Verse.AI;
using RimWorld;

namespace ProjectK9.AI
{
    public class ThinkNode_ConditionalTamed : ThinkNode_Conditional
    {
        public bool invertResult;

        public ThinkNode_ConditionalTamed()
        {
        }

        protected override bool Satisfied(Pawn pawn)
        {
            bool result = false;
            TameablePawn pet = pawn as TameablePawn;
            if (pet != null)
                result = pet.IsColonyPet;
            if (invertResult)
                result = !result;
            return result;
        }
    }
}
