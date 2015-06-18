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

        public static bool IsInHerd(Pawn pawn, Pawn herdMember)
        { 
            if ( pawn.Faction == GetColonyPetFaction()
              || herdMember.def != pawn.def
              || herdMember == pawn
              || !WanderUtility.InSameRoom(pawn.Position, herdMember.Position)
              || (pawn.Position - herdMember.Position).LengthHorizontalSquared > HERD_DISTANCE)
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
