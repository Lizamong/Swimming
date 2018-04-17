

using RimWorld;
using System;
using Verse;
using Verse.AI;

namespace Swimming
{
	public class JoyGiver_FindTreasure : JoyGiver
    {
        public override Job TryGiveJob(Pawn pawn)
        {
            if (!JoyUtility.EnjoyableOutsideNow(pawn, null))
            {
                return null;
            }

            IntVec3 c = JoyGiver_FindTreasure.TryFindTreasureCell(pawn);
            if (!c.IsValid)
            {
                return null;
            }
            return new Job(this.def.jobDef, c);
        }

        private static IntVec3 TryFindTreasureCell(Pawn pawn)
        {
            Region rootReg;
            if (!CellFinder.TryFindClosestRegionWith(pawn.GetRegion(RegionType.Set_Passable), 
                TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), 
                (Region r) => r.Room.PsychologicallyOutdoors, 100, out rootReg, RegionType.Set_Passable))
            {
                return IntVec3.Invalid;
            }
            IntVec3 result = IntVec3.Invalid;
            RegionTraverser.BreadthFirstTraverse(rootReg, (Region from, Region r) => r.Room
            == rootReg.Room, delegate (Region r)
            {
                for (int i = 0; i < 5; i++)
                {
                    IntVec3 randomCell = r.RandomCell;
                    if (JoyGiver_FindTreasure.IsGoodForSwimmingCell(randomCell, pawn))
                    {
                        result = randomCell;
                        return true;
                    }
                }
                return false;
            }, 30, RegionType.Set_Passable);
            return result;
        }

        private static bool IsGoodForSwimmingCell(IntVec3 c, Pawn pawn)
        {
            if (c.IsForbidden(pawn))
            {
                return false;
            }
            if (c.GetEdifice(pawn.Map) != null)
            {
                return false;
            }
            for (int i = 0; i < 9; i++)
            {
                IntVec3 c2 = c + GenAdj.AdjacentCellsAndInside[i];
                if (!c2.InBounds(pawn.Map))
                {
                    return false;
                }
                if (!c2.Standable(pawn.Map))
                {
                    return false;
                }
                //if (pawn.Map.reservationManager.IsReservedByAnyoneWhoseReservationsRespects(c2, pawn))
                //{
                //    return false;
                //}


                if (c2 is IntVec3 vec && vec.IsValid &&
                    vec.GetTerrain(pawn.MapHeld) is TerrainDef def &&
                  (def == TerrainDefOf.WaterDeep ||
                    def == TerrainDefOf.WaterMovingDeep ||
                    def == TerrainDefOf.WaterOceanDeep 
                    ))
                {
                    return true;

                    }
                }
            return false ;
        }
    }

}
