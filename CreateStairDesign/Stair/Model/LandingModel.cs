using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateStairDesign.Stair.Models
{

    public class LandingModel
   {
        public double Width { get; set; }
        public double Length { get; set; }
        public double Thickness { get; set; }
        public XYZ Start { get; set; }
        public XYZ End { get; set; }
        public LandingModel()
        {

        }
    }
}
