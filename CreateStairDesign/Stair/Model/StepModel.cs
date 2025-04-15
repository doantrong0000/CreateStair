using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateStairDesign.Stair.Models
{
   public class StepModel
   {
      public double Thickness { get; set; }
      public double StartLevel { get; set; }
      public double EndLevel { get; set; }
      public int StepNumber { get; set; }

      public double StepWidth { get; set; }

      public XYZ Start { get; set; }
      public XYZ End { get; set; }
      public Line Path { get; set; }

      public List<Line> Curves { get; set; }



   }
}
