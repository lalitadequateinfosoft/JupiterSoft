using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JupiterSoft.Models
{
   public class DeviceInfo
    {
        public List<CompleteDeviceInfo> CompleteDeviceInfos { get; set; }
        public List<CustomDeviceInfo> CustomDeviceInfos { get; set; }
    }

    public class CompleteDeviceInfo
    {
        public int Availability { get; set; }
        public string Caption { get; set; }
        public string ClassGuid { get; set; }
        public string[] CompatibleID { get; set; }
        public int ConfigManagerErrorCode { get; set; }
        public bool ConfigManagerUserConfig { get; set; }
        public string CreationClassName { get; set; }
        public string Description { get; set; }
        public string DeviceID { get; set; }
        public bool ErrorCleared { get; set; }
        public string ErrorDescription { get; set; }
        public string[] HardwareID { get; set; }
        public DateTime InstallDate { get; set; }
        public int LastErrorCode { get; set; }
        public string Manufacturer { get; set; }
        public string Name { get; set; }
        public string PortName { get; set; }
        public string PNPClass { get; set; }
        public string PNPDeviceID { get; set; }
        public int[] PowerManagementCapabilities { get; set; }
        public bool PowerManagementSupported { get; set; }
        public bool Present { get; set; }
        public string Service { get; set; }
        public string Status { get; set; }
        public int StatusInfo { get; set; }
        public string SystemCreationClassName { get; set; }
        public string SystemName { get; set; }
    }

    public class CustomDeviceInfo
    {
        public string Name { get; set; }
        public string PortName { get; set; }
        public string Manufacturer { get; set; }
        public string Caption { get; set; }
        public string DeviceID { get; set; }
    }
}
