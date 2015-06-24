using System;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace ProjectK9
{
    class PetApparelOverride : Pawn_ApparelTracker
    {
        public PetApparelOverride(Pawn pawn) : base(pawn)
        {
        }

        public new int WornApparelCount
        {
            get
            {
                return 1;
            }
        }

        public new bool PsychologicallyNude
        {
            get
            {
                return false;
            }
        }
    }
}
