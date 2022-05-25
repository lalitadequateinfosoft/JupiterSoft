using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JupiterSoft.Models
{
   public class Common
    {
        static Random random = new Random(5);

        public static UInt32 GetSessionNewId
        {
            get
            {
                return (UInt32)random.Next();
            }
        }

        public static UInt32 GetSessionId
        {
            get;
            set;
        }

        public static DateTime LastRequestSent
        {
            get;
            set;
        }
        
        public static DateTime LastResponseReceived
        {
            get;
            set;
        }

        public static DeviceType CurrentDevice
        {
            get;
            set;
        }

        public static string CurrentProperty
        {
            get;
            set;
        }

        public static UInt32 MaxRetryAttempt
        {
            get { return 1; }
        }

        public static UInt32 RetryAttempt
        {
            get;
            set;
        }


        public static COMType COMSelected
        {
            get;
            set;
        }

        public static Queue<byte[]> ReceiveBufferQueue = new Queue<byte[]>();

        public static Queue<RecData> ReceiveDataQueue = new Queue<RecData>();        

        public static List<RecData> RequestDataList = new List<RecData>();

        public static int RecState
        {
            get;
            set;
        }

        public static int RecIdx
        {
            get;
            set;
        }

        public static int SessionTimeOut
        {
            get;
            set;
            //get { return 2300; }  // ms  LinkBox
            //get { return 1000; }  // ms  USB
            //get { return 123000; }  //ms Debug
        }

        public static int DeviceWaitTimeOut
        {
            get;
            set;
        }

        public static int Address
        {
            get;
            set;
        }

        public static int BaudRate
        {
            get;
            set;
        }

        public static int Parity
        {
            get;
            set;
        }

        public static int StopBit
        {
            get;
            set;
        }

        public static string CompPort
        {
            get;
            set;
        }

        public static Byte[] MbTgmBytes
        {
            get;
            set;
        }

        public static int GoodTmgm
        {
            get;
            set;
        }

        public static int TimeOuts
        {
            get;
            set;
        }

        public static int CRCFaults
        {
            get;
            set;
        }

    }

    public class RecData
    {
        public UInt32 SessionId { get; set; }

        public DeviceType deviceType { get; set; }

        public string PropertyName { get; set; }

        public PortDataStatus Status { get; set; }

        public Byte[] MbTgm { get; set; }

        public RQType RqType { get; set; }

        public int Ch { get; set; }

        public int Indx { get; set; }

        public int Reg { get; set; }

        public int NoOfVal { get; set; }
    }

    public enum COMType
    {     
        UART,
        MODBUS,
        XYZ
    }

    public enum PortDataStatus
    {
        Requested,
        Ack,
        Busy,
        Received,
        SessionEnd,
        TimeOut,
        AckOkWait,
        Normal
    }

    public enum RQType
    {
        ModBus,
        MBus,
        WireLess,
        UART
    }

    public enum DeviceType
    {
        NetworkCamera,
        USBCamera,
        MotorDerive,
        WeightModule,
        ControlCard
    }

    public enum ModBus_Ack
    {
        OK,
        CrcFault,
        Timeout,
        Otherfault
    }

    public class SB1Handler
    {
        public enum SB1_STATE
        {
            IDLE,
            ACTIVE,
        };

        public enum SB1_CMD
        {
            END_SES,
            GET_SES_STATE,
            GET_LINK_INF,
            MODBUS_REQ,
            EnOcean_Listen_Start_REQ,
            EnOcean_Listen_End_REQ,
            MBUS_REQ,
        };

        public enum BAUD
        {
            BR_1200,
            BR_2400,
            BR_9600,
            BR_114200,
        };

        public enum SB1_Ack
        {
            OK,
            Busy,
            IllegalCommand,
            CrcFault,
            TgmFault,
            AckOkWait
        }

        SB1_STATE state = 0;
        SerialPort port;

        public bool SimSb1CrcFault = false;


        public SB1Handler(SerialPort sp)
        {
            port = sp;
        }       
    }

    public struct AppTools
    {
        public const string Modbus = "Modbus";
        public const string Mbus = "Mbus";
        public const string EnOcean = "EnOcean";
        public const string UART = "UART";
        public const string CleverHouse = "Clever House";
        public const string HC1SCC = "SC.CC.01";
        public const string SysemInformation = "SysemInformation";
        public const string None = "";
        public const string Sofcontrolsettings = "Sofcontrolsettings";
    }

}
