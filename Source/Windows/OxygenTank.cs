using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Swimming
{ 
    class OxygenTank : ThingComp
    {
        public override string CompInspectStringExtra()
        {
            StringBuilder lel = new StringBuilder();
            lel.Append(base.CompInspectStringExtra());
            lel.AppendLine("Oxygen "+Air.ToString()+ " / " +airMax.ToString());
            return lel.ToString();
        }
        public override void CompTick()
        {
            base.CompTick();
            IntVec3 intVec = this.parent.Position;
            if (intVec is IntVec3 vec && vec.IsValid &&
                vec.GetTerrain(this.parent.MapHeld) is TerrainDef def &&
              (def != TerrainDefOf.WaterDeep &&
                def != TerrainDefOf.WaterMovingDeep &&
                def != TerrainDefOf.WaterOceanDeep))
            {
                LoseAir();

            }
            else
            {
                GetAir();
            }
                }
        public void GetAir()
        {
            if (Find.TickManager.TicksGame % Props.ticksToGain == 0) ;
            {
                Air++;
            }
            if (Find.TickManager.TicksGame % Props.ticksToLose == 0) ;
            {
                Air--;
            }
        }
        public void LoseAir()
        {
    
        }
        public CompProperties_Oxygen Props
        {
            get
            {
                return (CompProperties_Oxygen)this.props;
            }
        }
        int air = 0;
        int airMax = 80;
        public int Air
        {
            get
            {
                return air;
            }
            set
            {
                air = Mathf.Clamp(value, 0, Props.airMax);
            }
        }
    }
}
