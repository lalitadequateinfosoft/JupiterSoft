using System;
using System.Collections.Generic;
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
}
