using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainPrecastToolBox.StairCreator.Models
{
   public class StepModel
   {
      public double Thickness { get; set; }
      public double StartLevel { get; set; }
      public double EndLevel { get; set; }
      public double StepNumber { get; set; }

      public XYZ Start { get; set; }
      public XYZ End { get; set; }
      public Line Path => Line.CreateBound(Start, End);

      public List<Line> Curves { get; set; }



   }
}
