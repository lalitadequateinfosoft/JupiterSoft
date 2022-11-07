using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JupiterSoft.Models
{
   public class RegisterConfiguration
    {
        public int RType { get; set; }
        public int DeviceType { get; set; }
        public int RegisterNo { get; set; }
        public decimal Frequency { get; set; }
        public int Count { get; set; }
    }
}
