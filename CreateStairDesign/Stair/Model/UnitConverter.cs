using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateStairDesign.Stair.Model
{
    public static class UnitConverter
    {
        public static double MmToFoot(this double mm)
        {
            return mm * 0.00328084;
        }
        public static double MmToFoot(this int mm)
        {
            return mm * 0.00328084;
        }
    }
}
