using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using Microsoft.Win32;
using JupiterSoft.Models;

namespace JupiterSoft.Models
{
  public  class DeviceInformation
    {
        public static DeviceInfo GetConnectedDevices()
        {
            DeviceInfo deviceInfo = new DeviceInfo();
            using (ManagementClass i_Entity = new ManagementClass("Win32_PnPEntity"))
            {
                List<CompleteDeviceInfo> completeDeviceInfos = new List<CompleteDeviceInfo>();
                List<CustomDeviceInfo> customDeviceInfos = new List<CustomDeviceInfo>();
                foreach (ManagementObject i_Inst in i_Entity.GetInstances())
                {
                    Object o_Guid = i_Inst.GetPropertyValue("ClassGuid");
                    if (o_Guid == null || o_Guid.ToString().ToUpper() != "{4D36E978-E325-11CE-BFC1-08002BE10318}")
                        continue; // Skip all devices except device class "PORTS"

                    var complete = GetCompleteDeviceInfo(i_Inst);
                    var custom = GetCustomDeviceInfo(i_Inst);

                    if(complete!=null && custom!=null)
                    {
                        completeDeviceInfos.Add(complete);
                        customDeviceInfos.Add(custom);
                    }
                }

                if(completeDeviceInfos!=null && completeDeviceInfos.Count()>0 && customDeviceInfos!=null && customDeviceInfos.Count()>0)
                {
                    deviceInfo.CompleteDeviceInfos = completeDeviceInfos;
                    deviceInfo.CustomDeviceInfos = customDeviceInfos;
                }
            }

            return deviceInfo;
        }

        public static CompleteDeviceInfo GetCompleteDeviceInfo(ManagementObject property)
        {
            CompleteDeviceInfo completeDevice = new CompleteDeviceInfo();
            try
            {
                completeDevice.Availability = property.GetPropertyValue("Availability") as int? ?? 0;
                completeDevice.Caption = property.GetPropertyValue("Caption") as string ?? string.Empty;
                completeDevice.ClassGuid = property.GetPropertyValue("ClassGuid") as string ?? string.Empty;
                completeDevice.CompatibleID = property.GetPropertyValue("CompatibleID") as string[] ?? new string[] { };
                completeDevice.ConfigManagerErrorCode = property.GetPropertyValue("ConfigManagerErrorCode") as int? ?? 0;
                completeDevice.ConfigManagerUserConfig = property.GetPropertyValue("ConfigManagerUserConfig") as bool? ?? false;
                completeDevice.CreationClassName = property.GetPropertyValue("CreationClassName") as string ?? string.Empty;
                completeDevice.Description = property.GetPropertyValue("Description") as string ?? string.Empty;
                completeDevice.DeviceID = property.GetPropertyValue("DeviceID") as string ?? string.Empty;
                completeDevice.ErrorCleared = property.GetPropertyValue("ErrorCleared") as bool? ?? false;
                completeDevice.ErrorDescription = property.GetPropertyValue("ErrorDescription") as string ?? string.Empty;
                completeDevice.HardwareID = property.GetPropertyValue("HardwareID") as string[] ?? new string[] { };
                completeDevice.InstallDate = property.GetPropertyValue("InstallDate") as DateTime? ?? DateTime.MinValue;
                completeDevice.LastErrorCode = property.GetPropertyValue("LastErrorCode") as int? ?? 0;
                completeDevice.Manufacturer = property.GetPropertyValue("Manufacturer") as string ?? string.Empty;
                completeDevice.Name = property.GetPropertyValue("Name") as string ?? string.Empty;
                completeDevice.PNPClass = property.GetPropertyValue("PNPClass") as string ?? string.Empty;
                completeDevice.PNPDeviceID = property.GetPropertyValue("PNPDeviceID") as string ?? string.Empty;
                completeDevice.PowerManagementCapabilities = property.GetPropertyValue("PowerManagementCapabilities") as int[] ?? new int[] { };
                completeDevice.PowerManagementSupported = property.GetPropertyValue("PowerManagementSupported") as bool? ?? false;
                completeDevice.Present = property.GetPropertyValue("Present") as bool? ?? false;
                completeDevice.Service = property.GetPropertyValue("Service") as string ?? string.Empty;
                completeDevice.Status = property.GetPropertyValue("Status") as string ?? string.Empty;
                completeDevice.StatusInfo = property.GetPropertyValue("StatusInfo") as int? ?? 0;
                completeDevice.SystemCreationClassName = property.GetPropertyValue("SystemCreationClassName") as string ?? string.Empty;
                completeDevice.SystemName = property.GetPropertyValue("SystemName") as string ?? string.Empty;

                String s_RegPath = "HKEY_LOCAL_MACHINE\\System\\CurrentControlSet\\Enum\\" + completeDevice.DeviceID + "\\Device Parameters";
                completeDevice.PortName = Registry.GetValue(s_RegPath, "PortName", "").ToString();

                int s32_Pos = completeDevice.Caption.IndexOf(" (COM");
                if (s32_Pos > 0) // remove COM port from description
                    completeDevice.Caption = completeDevice.Caption.Substring(0, s32_Pos);
            }
            catch
            {
                return new CompleteDeviceInfo();
            }
            return completeDevice;
        }

        public static CustomDeviceInfo GetCustomDeviceInfo(ManagementObject property)
        {
            CustomDeviceInfo customDevice = new CustomDeviceInfo();
            try
            {
                customDevice.Caption = property.GetPropertyValue("Caption").ToString();
                customDevice.Manufacturer = property.GetPropertyValue("Manufacturer").ToString();
                customDevice.DeviceID = property.GetPropertyValue("PnpDeviceID").ToString();
                String s_RegPath = "HKEY_LOCAL_MACHINE\\System\\CurrentControlSet\\Enum\\" + customDevice.DeviceID + "\\Device Parameters";
                customDevice.PortName = Registry.GetValue(s_RegPath, "PortName", "").ToString();

                int s32_Pos = customDevice.Caption.IndexOf(" (COM");
                if (s32_Pos > 0) // remove COM port from description
                    customDevice.Caption = customDevice.Caption.Substring(0, s32_Pos);
            }
            catch { return new CustomDeviceInfo(); }
           return customDevice;
        }

        public static List<System.Media.SystemSound> GetSystemSound()
        {
            var systemSounds = new List<System.Media.SystemSound>();
            systemSounds.Add(System.Media.SystemSounds.Asterisk);
            systemSounds.Add(System.Media.SystemSounds.Beep);
            systemSounds.Add(System.Media.SystemSounds.Exclamation);
            systemSounds.Add(System.Media.SystemSounds.Hand);
            systemSounds.Add(System.Media.SystemSounds.Question);

            return systemSounds;
        }
    }
}
