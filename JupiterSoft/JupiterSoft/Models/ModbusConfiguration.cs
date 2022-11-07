using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JupiterSoft.Models
{
   public class ModbusConfiguration
    {
        public string DeviceId { get; set; }
        public string PortName { get; set; }
        public int BaudRate { get; set; }
        public int DataBit { get; set; }
        public int StopBit { get; set; }
        public int Parity { get; set; }
        public SerialPort SerialDevice { get; set; }

        public RecData CurrentRequest { get; set; }
        public int RecIdx { get; set; }
        public int RecState { get; set; }
        public Queue<byte[]> ReceiveBufferQueue { get; set; }
        public bool IsComplete { get; set; }
        public DateTime LastResponseReceived
        {
            get;
            set;
        }

        public DateTime LastRequestSent
        {
            get;
            set;
        }

        public RegisterConfiguration MotorDrive { get; set; }
        public RegisterConfiguration PushingArm { get; set; }
        public RegisterConfiguration Sensor { get; set; }

        public bool IsConfigured { get; set; }
        public bool IsTurnedOn { get; set; }
    }
}
