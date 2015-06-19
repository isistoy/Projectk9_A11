using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;
using Verse.AI;
using RimWorld;

namespace ProjectK9
{
    class HerdUtility
    {
        public const int HERD_DISTANCE = 35 * 35;

        public static IEnumerable<Pawn> FindHerdMembers(Pawn pawn)
        {
            return Find.ListerPawns.AllPawns.Where(herdMember => IsInHerd(pawn, herdMember));
        }

        public static bool IsInHerd(Pawn pawn, Pawn target)
        {
            TameablePawn pet = pawn as TameablePawn;
            if (pet == null)
                return false;
            if ( pet.IsColonyPet
              || target.def != pawn.def
              || target == pawn
              || !WanderUtility.InSameRoom(pawn.Position, target.Position)
              || (pawn.Position - target.Position).LengthHorizontalSquared > HERD_DISTANCE)
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
