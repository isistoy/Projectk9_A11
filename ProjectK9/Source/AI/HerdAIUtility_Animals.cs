using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;
using Verse.AI;
using RimWorld;

namespace ProjectK9.AI
{
    public static class HerdAIUtility_Pets
    {
        public const int HERD_DISTANCE = 35 * 35;

        public static IEnumerable<Pawn> FindHerdMembers(Pawn pawn)
        {
            return Find.ListerPawns.AllPawns.Where(herdMember => IsInHerd(pawn, herdMember));
        }

        public static bool IsInHerd(Pawn herdMember, Pawn target)
        {
            TameablePawn tameableMember = herdMember as TameablePawn;
            if (tameableMember == null)
            {
                return false;
            }
            TameablePawn tameableTarget = target as TameablePawn;
            if (tameableTarget == null) 
            {
                return false;
            }
            if (tameableTarget.IsColonyPet 
                || tameableMember.IsColonyPet
                || target.def != herdMember.def
                || target == herdMember
                || !WanderUtility.InSameRoom(herdMember.Position, target.Position)
                || (herdMember.Position - target.Position).LengthHorizontalSquared > HERD_DISTANCE)
            {
                return false;
            }
            return true;
        }

        public static Faction GetColonyPetFaction()
        {
            return Find.FactionManager.FirstFactionOfDef(FactionDef.Named("ColonyPets"));
        }
    }
}
