using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace JupiterSoft.Models
{
   public class ApplicationConstant
    {
        public static readonly string _FileDirectory= @"C:\JupiterFiles";
        public static string CheckIPValid(string strIP)
        {
            if (string.IsNullOrEmpty(strIP)) return null;
            if (!strIP.Contains(".")) return null;
            if (strIP.Split('.').Length != 4) return null;
            IPAddress address;
            if (IPAddress.TryParse(strIP, out address))
            {
                switch (address.AddressFamily)
                {
                    case System.Net.Sockets.AddressFamily.InterNetwork:
                        // we have IPv4
                        return "ipv4";
                    //break;
                    case System.Net.Sockets.AddressFamily.InterNetworkV6:
                        // we have IPv6
                        return "ipv6";
                    //break;
                    default:
                        // umm... yeah... I'm going to need to take your red packet and...
                        return null;
                        //break;
                }
            }
            return null;
        }

        public static bool IsNumeric(string num)
        {
            var isNumeric = int.TryParse(num, out _);
            if (isNumeric) return true;
            return false;
        }
    }
}
