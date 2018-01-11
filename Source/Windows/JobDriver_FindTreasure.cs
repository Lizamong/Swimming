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
        public float Daniel ()
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
        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);

            int work = 0;
            int workmax = (int)(300 * Daniel());

            Toil GOD = Toils_General.Wait(300);
            GOD.tickAction = delegate
            {
                work++;
                if (work > workmax)
                {
                    this.ReadyForNextToil();
                }
            };
            GOD.WithProgressBar(TargetIndex.A, () => (float)(work / workmax));
            yield return GOD;

            Toil LOL = new Toil();
            LOL.initAction = delegate
            {
                List<ThingDef> Treasures = new List<ThingDef>()
            {
                ThingDefOf.Silver,ThingDefOf.Steel,
                DefDatabase<ThingDef>.AllDefs.ToList().FindAll(x =>x.IsRangedWeapon && x.BaseMarketValue<659 && x.graphicData!=null).RandomElement(),ThingDefOf.Gold,
                ThingDefOf.Plasteel

            };
                Thing DerpderdiDerpDerp = ThingMaker.MakeThing(Treasures.RandomElement());
                GenPlace.TryPlaceThing(DerpderdiDerpDerp, TargetLocA, GetActor().Map, ThingPlaceMode.Near);
            };
            LOL.WithProgressBarToilDelay(TargetIndex.A, workmax);
            yield return LOL;


        }
    }
}
