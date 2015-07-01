using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Verse;
using Verse.AI;
using UnityEngine;
using RimWorld;
using Backstories;
using System.Reflection;
using ProjectK9.AI;

namespace ProjectK9
{
    public class TameablePawn : Pawn, IThoughtGiver
    {
        private static Color pawnNameColor = new Color(0.9f, 0.9f, 0.9f);

        public TameablePawn_TameTracker tameTracker;

        public bool initialized;

        public bool IsColonyPet
        {
            get
            {
                if (tameTracker != null)
                    return tameTracker.IsTamed;
                return false;
            }
        }

        public override string LabelBase
        {
            get
            {
                if (IsColonyPet)
                    return (string.Concat(
                        this.Nickname,
                        ", ",
                        ageTracker.AgeNumberString,
                        ", Colony Pet")
                        );
                else
                    return base.LabelBase;
            }
        }

        public override string LabelBaseShort
        {
            get
            {
                if (IsColonyPet)
                    return this.Nickname;
                else
                    return base.LabelBaseShort;
            }
        }

        public override void DrawGUIOverlay()
        {
            if (IsColonyPet)
                drawPetOverlay();
            else
                base.DrawGUIOverlay();
        }

        private void drawPetOverlay()
        {
            if ((this.SpawnedInWorld && !Find.FogGrid.IsFogged(this.Position)))
            {
                Vector3 vector = (Vector3)GenWorldUI.LabelDrawPosFor(this, -0.6f);
                float y = vector.y;

                Text.Font = GameFont.Tiny;
                float x = GUI.skin.label.CalcSize(new GUIContent(this.LabelBaseShort)).x;
                if (x < 20f)
                    x = 20f;
                Rect position = new Rect((vector.x - (x / 2f)) - 4f, vector.y, x + 8f, 12f);
                GUI.DrawTexture(position, TexUI.GrayTextBG);
                if (this.health.summaryHealth.SummaryHealthPercent < 0.999f)
                    Widgets.FillableBar(position.ContractedBy(1f), this.health.summaryHealth.SummaryHealthPercent, PawnUIOverlay.HealthTex, BaseContent.ClearTex, false);
                GUI.color = pawnNameColor;
                Text.Anchor = TextAnchor.UpperCenter;
                Widgets.Label(new Rect(vector.x - (x / 2f), vector.y - 2f, x, 999f), this.LabelBaseShort);
                GUI.color = Color.white;
                Text.Anchor = TextAnchor.UpperLeft;
                y += 12f;
            }
        }

        //public override void TickLong()
        //{
        //    CleanupToughts();
        //    base.TickLong();
        //}

        private void CleanupToughts()
        {
            /// WIP            
            /// Fixed bad thoughts reference that we want out
            /// Should be loaded from an xml attribute collection
            /// 
            //if (this.initialized)
            //{
            //    // Get internal thought list
            //    //List<Thought> thoughtList = (List<Thought>)typeof(List<Thought>).GetField("thoughts", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(needs.mood.thoughts);
            //    // Reset existing with nothing
            //    typeof(List<Thought>).GetField("thoughts", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(needs.mood.thoughts, new List<Thought>());
            //}

            //List<ThoughtDef> forbidThoughtDefs = new List<ThoughtDef>();
            //forbidThoughtDefs.Add(ThoughtDef.Named("Naked"));
            //forbidThoughtDefs.Add(ThoughtDef.Named("ApparelDamaged"));
            //forbidThoughtDefs.Add(ThoughtDefOf.AteWithoutTable);
            //forbidThoughtDefs.Add(ThoughtDef.Named("AteRawFood"));
            //forbidThoughtDefs.Add(ThoughtDefOf.AteHumanlikeMeatDirect);
            //forbidThoughtDefs.Add(ThoughtDefOf.AteHumanlikeMeatAsIngredient);
            //forbidThoughtDefs.Add(ThoughtDefOf.ObservedLayingCorpse);
            //forbidThoughtDefs.Add(ThoughtDefOf.ObservedLayingRottingCorpse);
            //forbidThoughtDefs.Add(ThoughtDef.Named("SharedBedroom"));

            //var query =
            //    from c in needs.mood.thoughts.DistinctThoughtDefs
            //    join f in forbidThoughtDefs on c.defName equals f.defName
            //    select new { c };

            //foreach(ThoughtDef wrongDef in forbidThoughtDefs)
            //{
            //    IEnumerable<Thought> wrongThoughts = needs.mood.thoughts.ThoughtsOfDef(wrongDef);
            //}
        }

        public override void SpawnSetup()
        {
            base.SpawnSetup();
            if (!initialized)
            {
                TamePawnUtility.InitBasicPet(this);
                initialized = true;
            }
        }

        public override IEnumerable<FloatMenuOption> GetExtraFloatMenuOptionsFor(IntVec3 sq)
        {
            yield return new FloatMenuOption("testaction", new Action(() => { }));
        }

        public override void SetFaction(Faction newFaction)
        {
            mindState.Reset();
            pather.StopDead();
            jobs.StopAll();
            Find.ListerPawns.DeRegisterPawn(this);
            Find.Reservations.ReleaseAllClaimedBy(this);
            base.SetFactionDirect(Faction.OfColony);
            TamePawnUtility.GenerateStory(this);
            TamePawnUtility.InitWorkSettings(this);
            Reachability.ClearCache();
            if (playerController == null)
            {
                Log.Message("creating player controller");
                playerController = new Pawn_PlayerController(this);
            }
            // Potential other inits to do
            TamePawnUtility.CreateTameableMood(this);
            health.surgeryBills.Clear();
            Find.ListerPawns.RegisterPawn(this);
            Find.GameEnder.CheckGameOver();          
        }

        //public override void DeSpawn()
        //{
        //    if (ownership != null && ownership.ownedBed != null)
        //    {
        //        ownership.ownedBed.owner = PetBed.PetBedHolder;
        //        ownership = null;
        //    }
        //    base.DeSpawn();
        //}

        public override IEnumerable<Gizmo> GetGizmos()
        {
            if (!IsColonyPet)
            {
                Command_Toggle tame = new Command_Toggle();
                tame.defaultLabel = "Tame";
                tame.defaultDesc = "Tame this pet";
                tame.icon = ContentFinder<Texture2D>.Get("UI/Commands/Tame");
                tame.isActive = IsDesignatedToBeTamed;
                if (IsDesignatedToBeTamed())
                    tame.toggleAction = removePawnTamingDesignation;
                else
                    tame.toggleAction = addPawnTamingDesignation;

                tame.hotKey = KeyBindingDefOf.CommandItemForbid;
                yield return tame;
            }
            yield break;
        }

        public Thought GiveObservedThought()
        {
            int num1 = 10;
            int num2 = UnityEngine.Random.Range(1, 100);
            if (!IsColonyPet || (num2 < num1))
            {
                return null;
            }

            Thought_Observation obs = (Thought_Observation)ThoughtMaker.MakeThought(ThoughtDef.Named("SawCuteDog"));
            obs.Target = this;
            return obs;
        }

        public override void ExposeData()
        {
            //if (this.jobs != null && this.jobs.curDriver != null)
            //{
            //    this.jobs.EndCurrentJob(JobCondition.Errored);
            //}
            base.ExposeData();
            object[] objArray1 = new object[] { this };
            Scribe_Deep.LookDeep<TameablePawn_TameTracker>(ref this.tameTracker, "tameTracker", objArray1);
            Scribe_Values.LookValue<bool>(ref initialized, "initialized");
        }

        public bool IsDesignatedToBeTamed()
        {
            return getTamingDesignationOnSelf() != null;
        }

        public bool InPetBed()
        {
            return (CurrentPetBed() != null);
        }
        
        public PetBed CurrentPetBed()
        {
            if ((this.CurJob != null) && (this.CurJob.def == RestAIUtility_Animal.GetSleepJobDef()))
            {
                PetBed thing = this.jobs.curJob.targetA.Thing as PetBed;
                if ((thing != null) && (thing.CurPetOccupant == this))
                {
                    return thing;
                }
            }
            return null;
        }

        private void removePawnTamingDesignation()
        {
            Designation des = getTamingDesignationOnSelf();
            if (des != null)
                Find.DesignationManager.RemoveDesignation(des);
        }

        private Designation getTamingDesignationOnSelf()
        {
            return Find.DesignationManager.DesignationOn(this, DefDatabase<DesignationDef>.GetNamed("Tame"));
        }

        private void addPawnTamingDesignation()
        {
            Find.DesignationManager.AddDesignation(new Designation(this, DefDatabase<DesignationDef>.GetNamed("Tame")));
        }

        private WorkTypeDef getDef(string defName)
        {
            return DefDatabase<WorkTypeDef>.GetNamed(defName);
        }

        public override string ToString()
        {
            if (IsColonyPet)
            {
                return LabelBaseShort;
            }
            else
                return string.Concat("pet", this.thingIDNumber.ToString());
        }

        //protected override void ApplyDamage(DamageInfo dinfo)
        //{
        //    Log.Message(string.Concat(this, " taking ", dinfo.Amount, " damage. New health will be: ", (this.health - dinfo.Amount)));
        //    base.ApplyDamage(dinfo);
        //}

        //public override void Killed(DamageInfo dam)
        //{
        //    Log.Message(string.Concat("Killing: ", this));
        //    base.Killed(dam);
        //    Log.Message(string.Concat(this, " has been killed"));
        //}

        //public override void Destroy()
        //{
        //    Log.Message(string.Concat("Destroying: ", this));
        //    base.Destroy();
        //    Log.Message(string.Concat(this, " has been destroyed"));
        //}
    }
}