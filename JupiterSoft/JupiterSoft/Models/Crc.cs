using System;

namespace Components
{
    public static class AddedSum
    {
        public static Byte CalculateSum(byte[] buffer, int len)
        {
            Byte sum = 0;
            for (int k = 0; k < len; k++)
            {
                sum += buffer[k];
            }
            return sum;
        }
    }

    public static class CrcEnocean
    {
        public static bool CheckCrc(byte[] buffer, uint DataSize, byte cheksum)
        {
            byte[] u8CRC8Table = {
            0x00, 0x07, 0x0e, 0x09, 0x1c, 0x1b, 0x12, 0x15,
            0x38, 0x3f, 0x36, 0x31, 0x24, 0x23, 0x2a, 0x2d,
            0x70, 0x77, 0x7e, 0x79, 0x6c, 0x6b, 0x62, 0x65,
            0x48, 0x4f, 0x46, 0x41, 0x54, 0x53, 0x5a, 0x5d,
            0xe0, 0xe7, 0xee, 0xe9, 0xfc, 0xfb, 0xf2, 0xf5,
            0xd8, 0xdf, 0xd6, 0xd1, 0xc4, 0xc3, 0xca, 0xcd,
            0x90, 0x97, 0x9e, 0x99, 0x8c, 0x8b, 0x82, 0x85,
            0xa8, 0xaf, 0xa6, 0xa1, 0xb4, 0xb3, 0xba, 0xbd,
            0xc7, 0xc0, 0xc9, 0xce, 0xdb, 0xdc, 0xd5, 0xd2,
            0xff, 0xf8, 0xf1, 0xf6, 0xe3, 0xe4, 0xed, 0xea,
            0xb7, 0xb0, 0xb9, 0xbe, 0xab, 0xac, 0xa5, 0xa2,
            0x8f, 0x88, 0x81, 0x86, 0x93, 0x94, 0x9d, 0x9a,
            0x27, 0x20, 0x29, 0x2e, 0x3b, 0x3c, 0x35, 0x32,
            0x1f, 0x18, 0x11, 0x16, 0x03, 0x04, 0x0d, 0x0a,
            0x57, 0x50, 0x59, 0x5e, 0x4b, 0x4c, 0x45, 0x42,
            0x6f, 0x68, 0x61, 0x66, 0x73, 0x74, 0x7d, 0x7a,
            0x89, 0x8e, 0x87, 0x80, 0x95, 0x92, 0x9b, 0x9c,
            0xb1, 0xb6, 0xbf, 0xb8, 0xad, 0xaa, 0xa3, 0xa4,
            0xf9, 0xfe, 0xf7, 0xf0, 0xe5, 0xe2, 0xeb, 0xec,
            0xc1, 0xc6, 0xcf, 0xc8, 0xdd, 0xda, 0xd3, 0xd4,
            0x69, 0x6e, 0x67, 0x60, 0x75, 0x72, 0x7b, 0x7c,
            0x51, 0x56, 0x5f, 0x58, 0x4d, 0x4a, 0x43, 0x44,
            0x19, 0x1e, 0x17, 0x10, 0x05, 0x02, 0x0b, 0x0c,
            0x21, 0x26, 0x2f, 0x28, 0x3d, 0x3a, 0x33, 0x34,
            0x4e, 0x49, 0x40, 0x47, 0x52, 0x55, 0x5c, 0x5b,
            0x76, 0x71, 0x78, 0x7f, 0x6A, 0x6d, 0x64, 0x63,
            0x3e, 0x39, 0x30, 0x37, 0x22, 0x25, 0x2c, 0x2b,
            0x06, 0x01, 0x08, 0x0f, 0x1a, 0x1d, 0x14, 0x13,
            0xae, 0xa9, 0xa0, 0xa7, 0xb2, 0xb5, 0xbc, 0xbb,
            0x96, 0x91, 0x98, 0x9f, 0x8a, 0x8D, 0x84, 0x83,
            0xde, 0xd9, 0xd0, 0xd7, 0xc2, 0xc5, 0xcc, 0xcb,
            0xe6, 0xe1, 0xe8, 0xef, 0xfa, 0xfd, 0xf4, 0xf3
            };

            uint temp = 0;
            for (int i = 0; i < DataSize; i++)
            {
                temp = u8CRC8Table[temp ^ buffer[i]];
            }

            byte crc = (byte)(temp & 0x00ff);

            if (crc == cheksum)
            {
                return true;
            }

            else
            {
                return false;
            }
        }
    }

    public static class CrcModbus
    {
        public static void InsertCrc(byte[] buf, int len)
        {
            UInt16 crc = 0;
            Byte lo;
            Byte hi;
            crc = CalculateCrc(buf, len);
            lo = (Byte)(crc);
            hi = (Byte)(crc >> 8);
            buf[len + 0] = hi;
            buf[len + 1] = lo;
        }

        public static bool CheckCrc(byte[] buffer, int len)
        {
            bool retVal = false;
            try
            {
                Byte lo;
                Byte hi;
                UInt16 crc = CalculateCrc(buffer, len - 2);
                //UInt16 crc = ModRTU_CRC(buffer, len-2);//                

                lo = (Byte)crc;
                hi = (Byte)(crc >> 8);
                if (lo == buffer[len - 1] && hi == buffer[len - 2])
                {
                    retVal = true;
                }

            }
            catch
            {
            }
            return retVal;
        }

        private static UInt16 ModRTU_CRC(byte[] buf, int len)
        {
            UInt16 crc = 0xFFFF;

            for (int pos = 0; pos < len; pos++)
            {
                crc ^= (UInt16)buf[pos];          // XOR byte into least sig. byte of crc

                for (int i = 8; i != 0; i--)
                {    // Loop over each bit
                    if ((crc & 0x0001) != 0)
                    {      // If the LSB is set
                        crc >>= 1;                    // Shift right and XOR 0xA001
                        crc ^= 0xA001;
                    }
                    else                            // Else LSB is not set
                        crc >>= 1;                    // Just shift right
                }
            }
            // Note, this number has low and high bytes swapped, so use it accordingly (or swap bytes)
            return crc;
        }

        private static UInt16 CalculateCrc(byte[] buf, int len)
        {
            UInt16 crc16;
            UInt16 u16;
            crc16 = 0xffff;             //init the CRC WORD 
            for (int pos = 0; pos < len; pos++)
            {
                u16 = buf[pos];       //temp has the first byte 
                u16 &= 0x00FF;        //mask the MSB 
                crc16 = (UInt16)(crc16 ^ u16);      //crc16 XOR with temp 
                for (int c = 0; c < 8; c++)         // Loop over each bit
                {
                    if ((crc16 & 0x0001) != 0)              //If the LSB is set
                    {
                        crc16 = (UInt16)(crc16 >> 1);
                        crc16 = (UInt16)(crc16 ^ 0x0a001);  //Shift right and XOR 0xA001
                    }
                    else                                    //Else LSB is not set
                    {
                        crc16 = (UInt16)(crc16 >> 1);       //Just shift right
                    }
                }
            }
            crc16 = (UInt16)((crc16 >> 8) | (crc16 << 8));  // LSB is exchanged with MSB 
            return crc16;
        }
    }

    public class CrcOw
    {
        Byte crc_value;

        public CrcOw()
        {
            crc_value = 0x00;      //Initialize the crc_value
        }

        public void CrcStart()
        {
            crc_value = 0x00;
        }

        public void CrcUpdate(byte data)
        {
            Byte temp;
            temp = (Byte)(crc_value ^ data);
            crc_value = crc_tab[temp];
        }

        public ushort CrcEnd()
        {
            return (crc_value);
        }

        static readonly byte[] crc_tab =
        {
            0, 94,188,226, 97, 63,221,131,194,156,126, 32,163,253, 31, 65,
          157,195, 33,127,252,162, 64, 30, 95,  1,227,189, 62, 96,130,220,
           35,125,159,193, 66, 28,254,160,225,191, 93,  3,128,222, 60, 98,
          190,224,  2, 92,223,129, 99, 61,124, 34,192,158, 29, 67,161,255,
           70, 24,250,164, 39,121,155,197,132,218, 56,102,229,187, 89,  7,
          219,133,103, 57,186,228,  6, 88, 25, 71,165,251,120, 38,196,154,
          101, 59,217,135,  4, 90,184,230,167,249, 27, 69,198,152,122, 36,
          248,166, 68, 26,153,199, 37,123, 58,100,134,216, 91,  5,231,185,
          140,210, 48,110,237,179, 81, 15, 78, 16,242,172, 47,113,147,205,
           17, 79,173,243,112, 46,204,146,211,141,111, 49,178,236, 14, 80,
          175,241, 19, 77,206,144,114, 44,109, 51,209,143, 12, 82,176,238,
           50,108,142,208, 83, 13,239,177,240,174, 76, 18,145,207, 45,115,
          202,148,118, 40,171,245, 23, 73,  8, 86,180,234,105, 55,213,139,
           87,  9,235,181, 54,104,138,212,149,203, 41,119,244,170, 72, 22,
          233,183, 85, 11,136,214, 52,106, 43,117,151,201, 74, 20,246,168,
          116, 42,200,150, 21, 75,169,247,182,232, 10, 84,215,137,107, 53
        };
    }
}
