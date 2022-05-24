using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace Util
{
    public static class ConvertMisc
    {
        public static Byte[] ConvertUInt32BcdToByteArray(UInt32 u32)
        {
            Byte[] ret = new Byte[4];
            if (u32 < 100000000)
            {
                int decValue = Convert.ToInt32(u32.ToString(), 16);
                Byte[] a = BitConverter.GetBytes(decValue);
                ret[0] = a[3];
                ret[1] = a[2];
                ret[2] = a[1];
                ret[3] = a[0];
            }
            return ret;
        }
    }

    public static class ByteArrayConvert
    {
        // Convert BCD byte array to Int32 for M-BUS
        public static Int32 ConvertBCDToInt32(byte[] BCDS)
        {
            byte bcd = BCDS[BCDS.Length - 1];
            int bcdHi = bcd >> 4;
            Int32 result = 0;
            bool negativeValue = false;

            if (bcdHi == 0x0F)
            {
                BCDS[BCDS.Length - 1] = 0 >> 4;
                negativeValue = true;
            }

            for (int i = BCDS.Length - 1; i >= 0; i--)
            {
                byte bcdByte = BCDS[i];
                int idHigh = bcdByte >> 4;
                int idLow = bcdByte & 0x0F;
                if (idHigh > 9 || idLow > 9)
                {
                    throw new ArgumentException("Was not a valid binary-coded decimal format");
                }
                result = (result * 100) + idHigh * 10 + idLow;
            }

            if (negativeValue == true)
            {
                result = result * -1;
            }

            return result;
        }

        // Convert byte array to hex string
        public static string ByteArrayToHexString(byte[] ArrayToConvert, string Delimiter)
        {
            string[] BATHS = { "00", "01", "02", "03", "04", "05", "06", "07", "08", "09", "0A", "0B", "0C", "0D", "0E", "0F", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "1A", "1B", "1C", "1D", "1E", "1F", "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "2A", "2B", "2C", "2D", "2E", "2F", "30", "31", "32", "33", "34", "35", "36", "37", "38", "39", "3A", "3B", "3C", "3D", "3E", "3F", "40", "41", "42", "43", "44", "45", "46", "47", "48", "49", "4A", "4B", "4C", "4D", "4E", "4F", "50", "51", "52", "53", "54", "55", "56", "57", "58", "59", "5A", "5B", "5C", "5D", "5E", "5F", "60", "61", "62", "63", "64", "65", "66", "67", "68", "69", "6A", "6B", "6C", "6D", "6E", "6F", "70", "71", "72", "73", "74", "75", "76", "77", "78", "79", "7A", "7B", "7C", "7D", "7E", "7F", "80", "81", "82", "83", "84", "85", "86", "87", "88", "89", "8A", "8B", "8C", "8D", "8E", "8F", "90", "91", "92", "93", "94", "95", "96", "97", "98", "99", "9A", "9B", "9C", "9D", "9E", "9F", "A0", "A1", "A2", "A3", "A4", "A5", "A6", "A7", "A8", "A9", "AA", "AB", "AC", "AD", "AE", "AF", "B0", "B1", "B2", "B3", "B4", "B5", "B6", "B7", "B8", "B9", "BA", "BB", "BC", "BD", "BE", "BF", "C0", "C1", "C2", "C3", "C4", "C5", "C6", "C7", "C8", "C9", "CA", "CB", "CC", "CD", "CE", "CF", "D0", "D1", "D2", "D3", "D4", "D5", "D6", "D7", "D8", "D9", "DA", "DB", "DC", "DD", "DE", "DF", "E0", "E1", "E2", "E3", "E4", "E5", "E6", "E7", "E8", "E9", "EA", "EB", "EC", "ED", "EE", "EF", "F0", "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "FA", "FB", "FC", "FD", "FE", "FF" };

            int LengthRequired = (ArrayToConvert.Length + Delimiter.Length) * 2;
            StringBuilder tempstr = new StringBuilder(LengthRequired, LengthRequired);
            foreach (byte CurrentElem in ArrayToConvert)
            {
                tempstr.Append(BATHS[CurrentElem]);
                tempstr.Append(Delimiter);
            }

            return tempstr.ToString();
        }

        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        public static Int16 ToInt8(byte[] bytes, int idx)
        {
            Int32 u32 = 0;
            u32 += (Int32)bytes[idx++] << 0;
            return (Int16)u32;
        }

        public static Int16 ToInt16(byte[] bytes, int idx)
        {
            Int32 u32 = 0;
            u32 += (Int32)bytes[idx++] << 0;
            u32 += (Int32)bytes[idx++] << 8;
            return (Int16)u32;
        }

        public static Int32 ToInt24(byte[] bytes, int idx)
        {
            Int32 u32 = 0;
            u32 += (Int32)bytes[idx++] << 0;
            u32 += (Int32)bytes[idx++] << 8;
            u32 += (Int32)bytes[idx++] << 16;
            return u32;
        }

        public static Int32 ToInt32(byte[] bytes, int idx)
        {
            Int32 u32 = 0;
            u32 += (Int32)bytes[idx++] << 0;
            u32 += (Int32)bytes[idx++] << 8;
            u32 += (Int32)bytes[idx++] << 16;
            u32 += (Int32)bytes[idx++] << 24;
            return u32;
        }

        public static Int32 ToInt32Smapee(byte[] bytes, int idx)
        {
            Int32 u32 = 0;
            u32 += (Int32)bytes[idx++] << 8;
            u32 += (Int32)bytes[idx++] << 0;
            u32 += (Int32)bytes[idx++] << 24;
            u32 += (Int32)bytes[idx++] << 16;
            return u32;
        }

        public static Int64 ToInt48(byte[] bytes, int idx)
        {
            Int64 u64 = 0;
            u64 += (Int64)bytes[idx++] << 0;
            u64 += (Int64)bytes[idx++] << 8;
            u64 += (Int64)bytes[idx++] << 16;
            u64 += (Int64)bytes[idx++] << 24;
            u64 += (Int64)bytes[idx++] << 32;
            u64 += (Int64)bytes[idx++] << 40;
            return u64;
        }

        public static Int64 ToInt64(byte[] bytes, int idx)
        {
            Int64 u64 = 0;
            u64 += (Int64)bytes[idx++] << 0;
            u64 += (Int64)bytes[idx++] << 8;
            u64 += (Int64)bytes[idx++] << 16;
            u64 += (Int64)bytes[idx++] << 24;
            u64 += (Int64)bytes[idx++] << 32;
            u64 += (Int64)bytes[idx++] << 40;
            u64 += (Int64)bytes[idx++] << 48;
            u64 += (Int64)bytes[idx++] << 56;
            return u64;
        }

        public static UInt16 ToUInt16(byte[] bytes, int idx)
        {
            UInt32 u32 = 0;
            u32 += (UInt32)bytes[idx++] << 8;
            u32 += (UInt32)bytes[idx++] << 0;
            return (UInt16)u32;
        }

        public static UInt32 ToUInt32(byte[] bytes, int idx)
        {
            UInt32 u32 = 0;
            u32 += (UInt32)bytes[idx++] << 24;
            u32 += (UInt32)bytes[idx++] << 16;
            u32 += (UInt32)bytes[idx++] << 8;
            u32 += (UInt32)bytes[idx++] << 0;
            return u32;
        }

        public static UInt32 ToUInt32Smapee(byte[] bytes, int idx)
        {
            UInt32 u32 = 0;
            u32 += (UInt32)bytes[idx++] << 8;
            u32 += (UInt32)bytes[idx++] << 0;
            u32 += (UInt32)bytes[idx++] << 24;
            u32 += (UInt32)bytes[idx++] << 16;
            return u32;
        }

        public static UInt64 ToUInt64(byte[] bytes, int idx)
        {
            UInt64 u64 = 0;
            u64 += (UInt64)bytes[idx++] << 56;
            u64 += (UInt64)bytes[idx++] << 48;
            u64 += (UInt64)bytes[idx++] << 40;
            u64 += (UInt64)bytes[idx++] << 32;
            u64 += (UInt64)bytes[idx++] << 24;
            u64 += (UInt64)bytes[idx++] << 16;
            u64 += (UInt64)bytes[idx++] << 8;
            u64 += (UInt64)bytes[idx++] << 0;
            return u64;
        }

        public static Single ToSingle(byte[] bytes, int idx)
        {
            try
            {
                Byte[] val = new Byte[4];
                val[0] = bytes[3 + idx];
                val[1] = bytes[2 + idx];
                val[2] = bytes[1 + idx];
                val[3] = bytes[0 + idx];

                return BitConverter.ToSingle(val, 0);
            }
            catch (Exception ex)
            {
                StackTrace stackTrace = new StackTrace(ex, true);

                MethodBase methodBase;

                string FileName = stackTrace.GetFrame((stackTrace.FrameCount - 1)).GetFileName();

                Int32 lineNumber = stackTrace.GetFrame((stackTrace.FrameCount - 1)).GetFileLineNumber();

                FileName = FileName + ":Line No. " + lineNumber.ToString();

                methodBase = stackTrace.GetFrame((stackTrace.FrameCount - 1)).GetMethod();

                //These two lines are respnsible to find out name of the method                

                string MethodName = methodBase.Name;
                return 0;
            }
        }

        public static Single ToSingleSmapee(byte[] bytes, int idx)
        {
            try
            {
                Byte[] val = new Byte[4];
                val[0] = bytes[1 + idx];
                val[1] = bytes[0 + idx];
                val[2] = bytes[3 + idx];
                val[3] = bytes[2 + idx];

                return BitConverter.ToSingle(val, 0);
            }
            catch (Exception ex)
            {
                StackTrace stackTrace = new StackTrace(ex, true);

                MethodBase methodBase;

                string FileName = stackTrace.GetFrame((stackTrace.FrameCount - 1)).GetFileName();

                Int32 lineNumber = stackTrace.GetFrame((stackTrace.FrameCount - 1)).GetFileLineNumber();

                FileName = FileName + ":Line No. " + lineNumber.ToString();

                methodBase = stackTrace.GetFrame((stackTrace.FrameCount - 1)).GetMethod();

                //These two lines are respnsible to find out name of the method                

                string MethodName = methodBase.Name;
                return 0;
            }
        }

        public static Single ToSingleIskraT6(byte[] bytes, int idx)
        {
            Byte[] val = new Byte[4];
            val[0] = bytes[3 + idx];
            val[1] = bytes[2 + idx];
            val[2] = bytes[1 + idx];
            val[3] = bytes[0 + idx];

            Int32 i32 = BitConverter.ToInt32(val, 0);
            UInt32 u32_1 = (UInt32)i32;
            UInt32 u32_2 = u32_1 &= (UInt32)0x00FFFFFF;
            Single value = u32_2;
            sbyte exp = (sbyte)val[3];

            value = value * (Single)System.Math.Pow(10, exp);

            return value;
        }
    }

    public static class DateTimeCnv
    {
        public static float DateTimeToFloat32(DateTime dt)
        {
            UInt32 unixTime;
            DateTime unixStart = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            TimeSpan ts = dt - unixStart;
            long unixTimeStampInTicks = (long)(ts.Ticks / TimeSpan.TicksPerSecond);
            unixTime = (UInt32)unixTimeStampInTicks;

            byte[] array1 = BitConverter.GetBytes(unixTime);
            float f32 = BitConverter.ToSingle(array1, 0);
            return f32;
        }

        public static DateTime JsonToDt(string s)
        {
            DateTime dt = new DateTime(2000, 1, 1);
            int year;
            int month;
            int day;
            int hour;
            int minute;
            int sec;

            year = Convert.ToInt16(s.Substring(0, 4));
            month = Convert.ToInt16(s.Substring(5, 2));
            day = Convert.ToInt16(s.Substring(8, 2));
            hour = Convert.ToInt16(s.Substring(11, 2));
            minute = Convert.ToInt16(s.Substring(14, 2));
            sec = Convert.ToInt16(s.Substring(17, 2));

            try
            {
                dt = new DateTime(year, month, day, hour, minute, sec);
            }
            catch (Exception ex)
            {
                String exc = ex.Message;
                //Debug.Print(ex.Message);                
            }
            return dt;
        }
    }
}
