using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Swimming
{
    class CompProperties_Oxygen : CompProperties
    {
        public int ticksToGain = 120;
        public int ticksToLose = 150;
        public int airMax = 80;
        public CompProperties_Oxygen()
        {

            this.compClass = typeof(OxygenTank);
        }
    }
}
