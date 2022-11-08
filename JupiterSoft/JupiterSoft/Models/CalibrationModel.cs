using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JupiterSoft.Models
{
   public class CalibrationModel
    {
        public int id { get; set; }
        public string OutPutCom { get; set; }
        public decimal mVal { get; set; }
        public int command { get; set; }
        public string CommandText { get; set; }
    }
}
