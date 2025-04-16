using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CreateStairDesign.Stair.Model
{
    public enum StairType
    {
        Landing,
        Stair
    }

    public class XmlStair
    {
        public StairType Type { get; set; }

        // ID của phần tử
        public int ID { get; set; }

        // Cao độ đáy
        public double LowLevel { get; set; }

        // Cao độ đỉnh
        public double HighLevel { get; set; }

        // Bề dày
        public double Thickness { get; set; }

        // Số bậc (chỉ áp dụng với stair)
        public int StepNumber { get; set; }

        // Dùng để debug/truy vết hoặc nhóm theo tầng
        public string Tag => $"{Type}_{ID}";
        public override string ToString()
        {
            return $"{Type} [ID={ID}]\n" +
                   $"- LowLevel   : {LowLevel}\n" +
                   $"- HighLevel  : {HighLevel}\n" +
                   $"- Thickness  : {Thickness}\n" +
                   $"- StepNumber : {StepNumber}";
        }
    }

}
