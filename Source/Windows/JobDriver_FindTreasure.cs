using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using Verse.Sound;


namespace Swimming
{
    public class JobDriver_FindTreasure : JobDriver
    {
        public float Daniel()
        {
            Need_Rest rest = pawn.needs.rest;
            if (rest == null)
            {
                return 0f;
            }

            switch (rest.CurCategory)
            {
                case RestCategory.Rested:
                    return 1f;
                case RestCategory.Tired:
                    return 0.5f;
                case RestCategory.VeryTired:
                    return 0.125f;
                case RestCategory.Exhausted:
                    return 0.0000000000000000001f;
            }

            //if (rest.CurCategory < RestCategory.VeryTired)
            //{
            //    return true;
            //}
            return 0f;
        }

        int AnnoyingOrange = 0;
        int workmax => (int) (300 * Daniel());

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);


            Toil God = new Toil();
            God.initAction = delegate { AnnoyingOrange = workmax; };
            God.tickAction = delegate
            {
                if (AnnoyingOrange <= 0)
                {
                    this.ReadyForNextToil();
                }
                else
                {
                    AnnoyingOrange--;
                }
            };
            God.defaultCompleteMode = ToilCompleteMode.Never;
            God.WithProgressBar(TargetIndex.A, () => 1f - (float) AnnoyingOrange / workmax);
            yield return God;

            Toil LOL = new Toil();
            LOL.initAction = delegate
            {
                List<ThingDef> LowlevelTreasures = new List<ThingDef>()
                {
                    ThingDefOf.Silver,
                    ThingDefOf.Steel,
                };
                List<ThingDef> MiddlelevelTreasures = new List<ThingDef>()
                {
                   
                    DefDatabase<ThingDef>.AllDefs.ToList()
                        .FindAll(x =>
                            x.IsRangedWeapon && x.BaseMarketValue < 659 && x.graphicData != null).RandomElement(),
                    ThingDefOf.Gold,
                };
                MiddlelevelTreasures.AddRange(LowlevelTreasures);
                List<ThingDef> HighevelTreasures = new List<ThingDef>()
                {
                    ThingDefOf.Plasteel,
                    
                    DefDatabase<ThingDef>.AllDefs.ToList()
                    .FindAll(x =>
                    x.IsRangedWeapon && x.BaseMarketValue < 1000 && x.graphicData != null).RandomElement(),
                };

                    Thing DerpderdiDerpDerp = null;// = ThingMaker.MakeThing(Treasures.RandomElement());
                if (pawn.skills.skills.FirstOrDefault(x => x.def.defName == "Swimming")
                    is SkillRecord swimming)
                {
                    if (swimming.Level < 8) DerpderdiDerpDerp = ThingMaker.MakeThing(LowlevelTreasures.RandomElement());
                   else if (swimming.Level < 15) DerpderdiDerpDerp = ThingMaker.MakeThing(MiddlelevelTreasures.RandomElement());
                   else if (swimming.Level < 21) DerpderdiDerpDerp = ThingMaker.MakeThing(HighevelTreasures.RandomElement());
                }
                GenPlace.TryPlaceThing(DerpderdiDerpDerp, TargetLocA, GetActor().Map, ThingPlaceMode.Near);
            };
            LOL.WithProgressBarToilDelay(TargetIndex.A, workmax);
            yield return LOL;
        }

        public override bool TryMakePreToilReservations()
        {
            return true;
            //throw new NotImplementedException();
        }
    }
}