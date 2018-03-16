using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Swimming
{
    public  class Alert_LowOxygen : Alert
    {
        public static List<Pawn> IHateJellybeans;
        
        private const float NutritionThresholdPerColonist = 4f;

        public Alert_LowOxygen()
        {
            this.defaultLabel = "LowOxygen".Translate();
            this.defaultPriority = AlertPriority.High;
        }

        public override string GetExplanation()
        {
            Map map = IHateJellybeans?.FirstOrDefault()?.MapHeld;
            if (map ==                                                                         null)
            {
                return string.Empty;
            }

            return "OxygenLevel_Low".Translate();
        }

        public override AlertReport GetReport()
        {
            if (Find.TickManager.TicksGame < 150000)
            {
                return false;
            }    
            return IHateJellybeans != null;
        }

    }
}

