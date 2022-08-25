using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JupiterSoft.Models
{
    public class FileSystemModel
    {
        public string FileId { get; set; }
        public string FileName { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<FileContentModel> fileContents { get; set; }


    }
    public class FileContentModel
    {
        public string ContentId { get; set; }
        public int ContentType { get; set; }
        public string ContentText { get; set; }
        public string ContentValue { get; set; }
        public int ContentOrder { get; set; }
        public double ContentLeftPosition { get; set; }
        public double ContentTopPosition { get; set; }
    }

    public class userCommands
    {
        public string ContentId { get; set; }
        public int ContentType { get; set; }
        public string ContentText { get; set; }
        public string ContentValue { get; set; }
        public int ContentOrder { get; set; }
    }

    public class SelectedDevices
    {
        public string deviceId { get; set; }
        public int Baudrate { get; set; }
        public int databit { get; set; }
        public int stopbit { get; set; }
        public int parity { get; set; }
        public int TypeOfDevice { get; set; }
    }

    public class CommandExecutionModel
    {
        public List<userCommands> uCommands { get; set; }
        public List<SelectedDevices> sDevices { get; set; }
    }

    public class LogicalCommand
    {
        public string CommandId { get; set; }
        public int CommandType { get; set; }
        public int Order { get; set; }
        public int ExecutionStatus { get; set; }
        public DeviceConfiguration Configuration { get; set; }
        public ConditionDataModel InputData { get; set; }
        public RecData OutPutData { get; set; }
        public string CommandText { get; set; }
        public object CommandData { get; set; }
        public string ParentCommandId { get; set; }
    }

    public class RegisterWriteCommand
    {
        public int RegisterNumber { get; set; }
        public int RegisterOutput { get; set; }
    }

    public class ConditionDataModel
    {
        public string ComparisonVariable { get; set; }
        public int ComparisonCondition { get; set; }
        public string ComparisonValue { get; set; }
    }

    public class DeviceConfiguration
    {
        public DeviceSettings deviceDetail { get; set; }
        public CameraSettings cameraDetail { get; set; }

    }

    public class DeviceSettings
    {
        public string DeviceId { get; set; }
        public string PortName { get; set; }
        public int BaudRate { get; set; }
        public int DataBit { get; set; }
        public int StopBit { get; set; }
        public int Parity { get; set; }
    }
    public class CameraSettings
    {
        public int TypeOfCamera { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string Accessid { get; set; }
        public string AccessPassword { get; set; }
    }

    public enum CameraType
    {
        USB,
        IP_ONVIF,
        RTSP
    }

    public enum ExecutionStage
    {
        Not_Executed,
        Executing,
        Executed
    }

    public enum userDeviceType
    {
        NetworkCamera,
        USBCamera,
        MotorDerive,
        WeightModule,
        ControlCard
    }

    public enum weightUnit
    {
        KG,
        KGG,
        KGGM,
        LB,
        OZ,
        PCS
    }

    public enum OutPutType
    {
        SUCCESS,
        ERROR,
        INFORMATION,
        WARNING
    }
}
