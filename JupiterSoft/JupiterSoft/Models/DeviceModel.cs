using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JupiterSoft.Models
{
   public class DeviceModel
    {
        public string DeviceId { get; set; }
        public string Name { get; set; }
        public string DeviceIP { get; set; }
        public int DevicePort { get; set; }
        public bool Disconnected { get; set; }
        public bool Connected { get; set; }

        public string ConnectMessage { get; set; }
        public string DisconnectMessage { get; set; }
    }
}
