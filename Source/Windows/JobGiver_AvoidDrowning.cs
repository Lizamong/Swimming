using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse.AI;
using Verse;

namespace Swimming
{
    public class JobGiver_AvoidDrowning : ThinkNode_JobGiver
    {

        protected override Job TryGiveJob(Pawn pawn)
        {
            bool ispawnontheLand = pawn?.PositionHeld.GetTerrain(pawn.MapHeld)?.IsDeep() ?? false;
            bool isDrowning = pawn?.health?.hediffSet?.HasHediff(HediffDef.Named("LZG_Drowning")) ?? false;
            bool canDoJobNow = pawn?.CurJob?.def != JobDefOf.Goto &&
            pawn?.CurJob?.def !=  DefDatabase<JobDef>.GetNamed("LZG_FindTreasure");
            if (isDrowning && canDoJobNow && ispawnontheLand)
            {
               //if (Find.TickManager.TicksGame % 60 == 0)
               //{
                    IntVec3 White = FindOrangeLand(pawn);
                    if (White.IsValid)
                    {
                        Job Escape_from_eternal_death = new Job(JobDefOf.Goto, White);
                        Escape_from_eternal_death.locomotionUrgency = LocomotionUrgency.Sprint;
                        return Escape_from_eternal_death;
                    }
               //}
            }
            return null;
        }
        private Pawn pawn;
       private IntVec3 FindOrangeLand(Pawn pawn)
        {
            IntVec3 position = pawn.Position;
            for (int i = 0; i < 24; i++)
            {
                IntVec3 intVec = position + GenRadial.RadialPattern[i];
                if (intVec.InBounds(pawn.Map))
                {
                    if (intVec is IntVec3 vec && vec.IsValid &&
                        vec.GetTerrain(pawn.MapHeld) is TerrainDef def &&
                      (def != TerrainDefOf.WaterDeep &&
                        def != TerrainDefOf.WaterMovingDeep &&
                        def != TerrainDefOf.WaterOceanDeep ))
                    {

                        if (GenSight.LineOfSight(position, intVec, pawn.Map, false, null, 0, 0))
                        {
                            Log.Message(intVec.ToString());
                            return intVec;
                        }
                    }

                }
            }
            Log.Message(IntVec3.Invalid.ToString());
            return IntVec3.Invalid;
        }

    }
}
