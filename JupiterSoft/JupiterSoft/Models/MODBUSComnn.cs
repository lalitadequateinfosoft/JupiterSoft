using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JupiterSoft.Models
{
    class MODBUSComnn
    {
        public void SetMultiSendorValueFM16(int Adr, int Parity, SerialPort port, int register, int val, string PName, int Ch, int Indx, DeviceType deviceType, int[] byteArr, bool IsReading)
        {
            if (!IsReading)
            {                
                PrepareHC5MRRequestFM16(Adr, 16, register-1 >> 8, register-1, port, val, PName, Ch, Indx, deviceType, byteArr);
            }
        }

        public void SetMultiSendorValueFM6(int Adr, int Parity, SerialPort port, int register, int val, string PName, bool IsReading)
        {
            if (!IsReading)
            {                
                PrepareHC5MRRequest(Adr, 6, register - 1 >> 8, register - 1, port, val, PName);
            }
        }

        public void GetMultiSendorValueFM3(int Adr, int Parity, SerialPort port, int register, int val, string PName, int Ch, int Indx, DeviceType deviceType)
        {
            PrepareHC5MRRequestMulti(Adr, 3, register  >> 8, register, port, val, PName, Ch, Indx, deviceType);
        }

        public void GetMultiSendorValueFM4(int Adr, int Parity, SerialPort port, int register, int val, string PName, int Ch, int Indx, DeviceType deviceType)
        {
            PrepareHC5MRRequestMulti(Adr, 4, register >> 8, register, port, val, PName, Ch, Indx, deviceType);
        }

        private void PrepareHC5MRRequestFM16(int Adr, int FCode, int Hi, int Lo, SerialPort port, int val, string PName, int Ch, int Indx, DeviceType deviceType, int[] byteArr)
        {
            RecData _recData = new RecData();
            _recData.deviceType = deviceType;
            _recData.PropertyName = PName;
            _recData.SessionId = Common.GetSessionNewId;
            _recData.Ch = Ch;
            _recData.Indx = Indx;
            _recData.Reg = Lo;
            _recData.NoOfVal = val;
            Common.GetSessionId = _recData.SessionId;
            _recData.Status = PortDataStatus.Requested;
            _recData.RqType = RQType.ModBus;
            Common.RequestDataList.Add(_recData);

            Common.DeviceWaitTimeOut = 50;
            MbTgm _Mbtg = new MbTgm();
            _Mbtg.GenerateMbTgmFC16(Adr, FCode, Hi, Lo, val, byteArr);

            PayloadRQ _PayloadRQ = new PayloadRQ();
            _PayloadRQ.mbTgmBytes = _Mbtg.MbTgmData;
            _PayloadRQ.MbTgmLength = (ushort)_Mbtg.MbTgmData.Length;
            //_PayloadRQ.Channel = 1;
            _PayloadRQ.Baud = Common.BaudRate;
            _PayloadRQ.Parity = (Byte)Common.Parity;
            _PayloadRQ.StopBit = (Byte)Common.StopBit;
            _PayloadRQ.ModBusPayload();

            SB1Request _RqSB1 = new SB1Request();
            _RqSB1.cmd = (int)SB1Handler.SB1_CMD.MODBUS_REQ;
            _RqSB1.crc = 2; // Common.GetSessionId;
            _RqSB1.length = (UInt32)_PayloadRQ.MbTgmLength + 44;
            _RqSB1.payloadByte = _PayloadRQ.ModBusPayloadRq;
            _RqSB1.payLoadSize = Convert.ToUInt32(_PayloadRQ.MbTgmLength) + 12;
            _RqSB1.sesId = Common.GetSessionId;
            _RqSB1.payload = _PayloadRQ;
            //Common.WriteLog("Request :- " + _RqSB1.sesId.ToString());
            //Common.GetSessionId = _RqSB1.sesId;
            Common.LastRequestSent = DateTime.Now;
            Common.RecState = 1;
            _RqSB1.Tx(port);
            Thread.Sleep(Common.DeviceWaitTimeOut);
        }

        private void PrepareHC5MRRequest(int Adr, int FCode, int Hi, int Lo, SerialPort port, int val, string PName)
        {
            RecData _recData = new RecData();
            // _recData.deviceTypeId = DeviceType.SCIM04;
            _recData.PropertyName = PName;
            _recData.SessionId = Common.GetSessionNewId;
            Common.GetSessionId = _recData.SessionId;
            _recData.Status = PortDataStatus.Requested;
            _recData.RqType = RQType.ModBus;
            Common.RequestDataList.Add(_recData);

            Common.DeviceWaitTimeOut = 50;
            MbTgm _Mbtg = new MbTgm();
            _Mbtg.GenerateMbTgm(Adr, FCode, Hi, Lo, val);

            PayloadRQ _PayloadRQ = new PayloadRQ();
            _PayloadRQ.mbTgmBytes = _Mbtg.MbTgmData;
            _PayloadRQ.MbTgmLength = (ushort)_Mbtg.MbTgmData.Length;
            //_PayloadRQ.Channel = 1;
            _PayloadRQ.Baud = Common.BaudRate;
            _PayloadRQ.Parity = (Byte)Common.Parity;
            _PayloadRQ.StopBit = (Byte)Common.StopBit;
            _PayloadRQ.ModBusPayload();

            SB1Request _RqSB1 = new SB1Request();
            _RqSB1.cmd = (int)SB1Handler.SB1_CMD.MODBUS_REQ;
            _RqSB1.crc = 2; // Common.GetSessionId;
            _RqSB1.length = (UInt32)_PayloadRQ.MbTgmLength + 44;
            _RqSB1.payloadByte = _PayloadRQ.ModBusPayloadRq;
            _RqSB1.payLoadSize = Convert.ToUInt32(_PayloadRQ.MbTgmLength) + 12;
            _RqSB1.sesId = Common.GetSessionId;
            _RqSB1.payload = _PayloadRQ;
            //Common.WriteLog("Request :- " + _RqSB1.sesId.ToString());
            //Common.GetSessionId = _RqSB1.sesId;
            Common.LastRequestSent = DateTime.Now;
            Common.RecState = 1;
            _RqSB1.Tx(port);
            Thread.Sleep(Common.DeviceWaitTimeOut);
        }

        private void PrepareHC5MRRequestMulti(int Adr, int FCode, int Hi, int Lo, SerialPort port, int val, string PName, int Ch, int Indx, DeviceType deviceType)
        {
            RecData _recData = new RecData();
            _recData.deviceType = deviceType;
            _recData.PropertyName = PName;
            _recData.SessionId = Common.GetSessionNewId;
            _recData.Ch = Ch;
            _recData.Indx = Indx;
            _recData.Reg = Lo;
            _recData.NoOfVal = val;
            Common.GetSessionId = _recData.SessionId;
            _recData.Status = PortDataStatus.Requested;
            _recData.RqType = RQType.ModBus;
            Common.RequestDataList.Add(_recData);

            Common.DeviceWaitTimeOut = 50;
            MbTgm _Mbtg = new MbTgm();
            _Mbtg.GenerateMbTgm(Adr, FCode, Hi, Lo, val);

            PayloadRQ _PayloadRQ = new PayloadRQ();
            _PayloadRQ.mbTgmBytes = _Mbtg.MbTgmData;
            _PayloadRQ.MbTgmLength = (ushort)_Mbtg.MbTgmData.Length;
            //_PayloadRQ.Channel = 1;
            _PayloadRQ.Baud = Common.BaudRate;
            _PayloadRQ.Parity = (Byte)Common.Parity;
            _PayloadRQ.StopBit = (Byte)Common.StopBit;
            _PayloadRQ.ModBusPayload();

            SB1Request _RqSB1 = new SB1Request();
            _RqSB1.cmd = (int)SB1Handler.SB1_CMD.MODBUS_REQ;
            _RqSB1.crc = 2; // Common.GetSessionId;
            _RqSB1.length = (UInt32)_PayloadRQ.MbTgmLength + 44;
            _RqSB1.payloadByte = _PayloadRQ.ModBusPayloadRq;
            _RqSB1.payLoadSize = Convert.ToUInt32(_PayloadRQ.MbTgmLength) + 12;
            _RqSB1.sesId = Common.GetSessionId;
            _RqSB1.payload = _PayloadRQ;
            //Common.WriteLog("Request :- " + _RqSB1.sesId.ToString());
            //Common.GetSessionId = _RqSB1.sesId;
            Common.LastRequestSent = DateTime.Now;
            Common.RecState = 1;
            _RqSB1.Tx(port);
            Thread.Sleep(Common.DeviceWaitTimeOut);
        }
    }

    public class MbTgm
    {
        public Byte SlaveAddress;
        public Byte FunctionCode;
        public Byte StartAddressHi;
        public Byte StartAddressLo;
        public Byte FirstRegisterHi;
        public Byte FirstRegisterLo;
        public Byte SecondRegisterHi;
        public Byte SecondRegisterLo;
        public Byte ErrorCode;
        public Byte NOPHi;
        public Byte NOPLo0;
        public Byte NOPLo1;
        public Byte CRCHi;
        public Byte CRCLo;
        public bool StartBit;
        public bool StopBit;
        public bool ParityBit;
        public Byte[] MbTgmData;
        public Byte[] GenerateMbTgm(int Adr, int FCode, int Hi, int Lo, int val)
        {

            Byte[] tx = new Byte[8];
            tx[0] = (Byte)Adr; // Slave Address
            tx[1] = (Byte)FCode; //Function Code
            tx[2] = (Byte)Hi;  // HI Address
            tx[3] = (Byte)Lo;  // LO Address
            tx[4] = (Byte)(val >> 8);  // No of point Hi
            tx[5] = (Byte)val; // No of point Lo
            Components.CrcModbus.InsertCrc(tx, 6);
            MbTgmData = tx;
            return tx;
        }

        public Byte[] GenerateMbTgmFC16(int Adr, int FCode, int Hi, int Lo, int NoOfVal, int[] Values)
        {

            Byte[] tx = new Byte[NoOfVal * 2 + 9];
            tx[0] = (Byte)Adr; // Slave Address
            tx[1] = (Byte)FCode; //Function Code
            tx[2] = (Byte)Hi;  // HI Address
            tx[3] = (Byte)Lo;  // LO Address
            tx[4] = (Byte)(NoOfVal >> 8);  // No of point Hi
            tx[5] = (Byte)NoOfVal; // No of point Lo
            tx[6] = (Byte)(NoOfVal * 2);  //Multile of Two for 16 bit Register

            //for (int i = 1, j = 0; i <= NoOfVal * 2; i = i + 2, j++)
            //{
            //    tx[6 + i] = (Byte)(Values[j]);
            //    //Uncomment for test
            //    //tx[6 + i] = (Byte)(Values[j] >> 8);
            //    //tx[7 + i] = (Byte)Values[j];
            //}

            for (int i = 1; i <= NoOfVal * 2; i++)
            {
                tx[6 + i] = (Byte)(Values[i - 1]);
            }

            Components.CrcModbus.InsertCrc(tx, 7 + NoOfVal * 2);
            MbTgmData = tx;
            return tx;
        }


        public static ModBus_Ack GetModBusAck(Byte[] PLTgm)
        {
            if (PLTgm[0] == 0)
            {
                return ModBus_Ack.OK;
            }
            else if (PLTgm[0] == 1)
            {
                return ModBus_Ack.CrcFault;
            }
            else if (PLTgm[0] == 2)
            {
                return ModBus_Ack.CrcFault;
            }
            else if (PLTgm[0] == 3)
            {
                return ModBus_Ack.Otherfault;
            }
            else
            {
                return ModBus_Ack.Otherfault;
            }
        }


    }

    public class ModBus
    {
        SerialPort port;
        //OutputPort direction;
        Byte[] tx = new Byte[8];
        Byte[] rx = new Byte[100];

        //TODO CRC
        public ModBus(SerialPort com)
        {
            //direction = p;
            port = com;
            if (!port.IsOpen)
            {
                port.Open();
            }
        }

        public Byte[] GetReply()
        {
            return rx;
        }       
    }

    public class PayloadRQ
    {
        //public Byte Channel;
        public Int32 Baud;
        public Byte Parity;
        public Byte StopBit;
        public UInt16 MbTgmLength;
        public UInt32 Reserved;
        public MbTgm mbTgm;
        public Byte[] mbTgmBytes;
        public Byte[] ModBusPayloadRq;

        public Byte[] ModBusPayload()
        {
            Byte[] txPL = new Byte[1000];
            Array.Clear(txPL, 0, 1000);
            Byte[] a;
            a = BitConverter.GetBytes(Baud);
            Array.Reverse(a);
            Array.Copy(a, 0, txPL, 0, 4);
            //a = BitConverter.GetBytes(Parity);
            //Array.Reverse(a);
            //Array.Copy(a, 0, txPL, 4, 1);
            //a = BitConverter.GetBytes(StopBit);
            //Array.Reverse(a);
            //Array.Copy(a, 0, txPL, 5, 1);
            txPL[4] = Parity;
            txPL[5] = StopBit;
            a = BitConverter.GetBytes(MbTgmLength);
            Array.Reverse(a);
            Array.Copy(a, 0, txPL, 6, 2);
            a = BitConverter.GetBytes(Reserved);
            Array.Reverse(a);
            Array.Copy(a, 0, txPL, 8, 4);
            Array.Copy(mbTgmBytes, 0, txPL, 12, MbTgmLength);
            ModBusPayloadRq = txPL;
            return txPL;
        }
    }

    public class PayloadRS
    {
        //public Byte Channel;
        public Byte MbAck;
        public UInt16 MbLength;
        public UInt16 Reserved;
        public MbTgm mbTgm;
        public Byte[] mbTgmBytes;

        public void SetPayLoadRS(Byte[] payLoad)
        {
            MbAck = (Byte)payLoad[0];
            MbLength = Util.ByteArrayConvert.ToUInt16(payLoad, 1);
            Reserved = Util.ByteArrayConvert.ToUInt16(payLoad, 3);
        }

        public Byte[] ExtractModBusTgm(Byte[] payLoad)
        {
            Byte[] _MbTgm = new Byte[1000];
            Array.Copy(payLoad, 5, _MbTgm, 0, MbLength);
            mbTgmBytes = _MbTgm;
            return _MbTgm;
        }
    }

    public class SB1Request
    {
        public UInt32 length;
        public UInt16 cmd;
        public UInt32 sesId;
        public UInt32 payLoadSize;
        public UInt32 Reserved = 16;
        public PayloadRQ payload;
        public Byte[] payloadByte;
        public UInt16 crc;
        Byte[] raw = new Byte[1000];

        public void SetTgm(Byte[] tgm, int len)
        {
            length = Util.ByteArrayConvert.ToUInt32(tgm, 0);
            cmd = Util.ByteArrayConvert.ToUInt16(tgm, 4);
            sesId = Util.ByteArrayConvert.ToUInt32(tgm, 6);
            payLoadSize = Util.ByteArrayConvert.ToUInt32(tgm, 10);
            crc = Util.ByteArrayConvert.ToUInt16(tgm, 30 + (int)payLoadSize);
            raw = tgm;
        }

        public bool CheckCrc()
        {
            return Components.CrcModbus.CheckCrc(raw, (int)length);
        }

        public void Tx(SerialPort sp)
        {
            //sp.DiscardInBuffer();
            //sp.DiscardOutBuffer();
            try
            {
                Array.Clear(raw, 0, 1000);

                Byte[] a;
                a = BitConverter.GetBytes(length);
                Array.Reverse(a);
                Array.Copy(a, 0, raw, 0, 4);
                a = BitConverter.GetBytes(cmd);
                Array.Reverse(a);
                Array.Copy(a, 0, raw, 4, 2);
                a = BitConverter.GetBytes(sesId);
                Array.Reverse(a);
                Array.Copy(a, 0, raw, 6, 4);
                a = BitConverter.GetBytes(payLoadSize);
                Array.Reverse(a);
                Array.Copy(a, 0, raw, 10, 4);
                Array.Copy(payloadByte, 0, raw, 30, payLoadSize);
                Components.CrcModbus.InsertCrc(raw, (int)length - 2);
                if (Common.COMSelected == COMType.XYZ)
                {
                    sp.Write(raw, 0, (int)length);
                }
                else if (Common.COMSelected == COMType.MODBUS)
                {
                    if (payload != null)
                        sp.Write(payload.mbTgmBytes, 0, (int)payload.MbTgmLength);
                }

            }
            catch (Exception ex)
            {
                var a = ex;
            }
        }

        public void EndSession(SerialPort port)
        {
            //Common.ReceiveSizePB = Common.MaxReceiveSizePB; // Complete the operation

            SB1Request _RqSB1 = new SB1Request();
            _RqSB1.cmd = (int)SB1Handler.SB1_CMD.END_SES;
            _RqSB1.crc = 2; // Common.GetSessionId;
            _RqSB1.length = 32;
            _RqSB1.payLoadSize = 0;
            _RqSB1.payloadByte = new Byte[0];
            _RqSB1.sesId = Common.GetSessionId;
            Common.LastRequestSent = DateTime.Now;
            Common.RecState = 1;
            _RqSB1.Tx(port);
            //Common.WriteLog("Request :- " + "EndSess" + "-" + _RqSB1.sesId.ToString());
            //Common.RequestDataList.Remove(Common.RequestDataList.Where(a => a.SessionId == Common.GetSessionId).FirstOrDefault());
            //Common.GetSessionId = 0; // End Session
            Common.RecIdx = 0;
            //Common.RetryAttempt++;          
            

        }

        public void GetSessionState(SerialPort port)
        {
            SB1Request _RqSB1 = new SB1Request();
            _RqSB1.cmd = (int)SB1Handler.SB1_CMD.GET_SES_STATE;
            _RqSB1.crc = 2; // Common.GetSessionId;
            _RqSB1.length = 32;
            _RqSB1.payLoadSize = 0;
            _RqSB1.sesId = Common.GetSessionId;
            Common.LastRequestSent = DateTime.Now;
            _RqSB1.Tx(port);
        }
    }

    public class SB1Reply
    {
        public UInt32 length;
        public UInt16 ack;
        public UInt32 sesId;
        public UInt32 payLoadSize;
        public Byte[] payload;
        public Byte[] RxSB1;
        public UInt16 crc;

        public SB1Reply(UInt32 id)
        {
            sesId = id;
        }

        public void SetTgm(Byte[] tgm)
        {
            if (Common.COMSelected == COMType.XYZ)
            {
                length = Util.ByteArrayConvert.ToUInt32(tgm, 0);
                ack = Util.ByteArrayConvert.ToUInt16(tgm, 4);
                sesId = Util.ByteArrayConvert.ToUInt32(tgm, 6);
                payLoadSize = Util.ByteArrayConvert.ToUInt32(tgm, 10);
                crc = Util.ByteArrayConvert.ToUInt16(tgm, 30 + (int)payLoadSize);
                RxSB1 = tgm;
            }
            else if (Common.COMSelected == COMType.MODBUS)
            {

                length = (uint)tgm[2] + 5;
                //ack = Util.ByteArrayConvert.ToUInt16(tgm, 4);
                //sesId = Util.ByteArrayConvert.ToUInt32(tgm, 6);
                //payLoadSize = Util.ByteArrayConvert.ToUInt32(tgm, 10);
                //crc = Util.ByteArrayConvert.ToUInt16(tgm, 30 + (int)payLoadSize);
                RxSB1 = tgm;
            }
        }

        public void SetTgm(Byte[] tgm, string CurrentActiveMenu)
        {
            if (Common.COMSelected == COMType.XYZ)
            {
                length = Util.ByteArrayConvert.ToUInt32(tgm, 0);
                ack = Util.ByteArrayConvert.ToUInt16(tgm, 4);
                sesId = Util.ByteArrayConvert.ToUInt32(tgm, 6);
                payLoadSize = Util.ByteArrayConvert.ToUInt32(tgm, 10);
                crc = Util.ByteArrayConvert.ToUInt16(tgm, 30 + (int)payLoadSize);
                RxSB1 = tgm;
            }
            else if (Common.COMSelected == COMType.MODBUS)
            {
                if (CurrentActiveMenu == AppTools.Modbus)
                {
                    length = (uint)tgm[2] + 5;
                    payLoadSize = 0;
                    RxSB1 = tgm;
                }
                else if (CurrentActiveMenu == AppTools.EnOcean)
                {
                    byte[] Temp = new byte[4];
                    Temp[2] = tgm[1];
                    Temp[3] = tgm[2];
                    UInt32 LenghtData = Util.ByteArrayConvert.ToUInt32(Temp, 0);
                    Temp[3] = tgm[3];
                    UInt32 LengthOptional = Util.ByteArrayConvert.ToUInt32(Temp, 0);
                    length = LenghtData + LengthOptional + 7;
                }
            }
        }



        public void InsertPayload(Byte[] pl, int len)
        {
            length = 32 + (UInt32)len;
            ack = 0;
            payLoadSize = (UInt32)len;
            Array.Copy(pl, 0, payload, 0, len);
        }

        public Byte[] ExtractPayload()
        {
            Byte[] _payload = new Byte[1000];
            Array.Copy(RxSB1, 30, _payload, 0, payLoadSize);
            payload = _payload;
            return payload;
        }

        public bool CheckCrc(Byte[] SBData, int len)
        {
            return Components.CrcModbus.CheckCrc(SBData, len);
        }

        public void NackIllCmd()
        {
            length = 32;
            ack = 2;
            payLoadSize = 0;
        }

        public void AckOk()
        {
            length = 32;
            ack = 0;
            payLoadSize = 0;
        }

        public bool IsAckOk()
        {
            if (ack == 0 || ack == 5)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        public void AckBusy()
        {
            length = 32;
            ack = 1;
            payLoadSize = 0;
        }

        public void NackCrcFault()
        {
            length = 32;
            ack = 3;
            payLoadSize = 0;
        }
    }


}
