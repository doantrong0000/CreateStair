using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainPrecastToolBox.StairCreator.Models
{
   internal class StairModel
   {
      public StepModel Step1 { get; set; }
      public StepModel Step2 { get; set; }
      public StepModel Step3 { get; set; }
      public LandingModel Landing { get; set; }

      public StairModel()
      {
         

      }

   }
}
