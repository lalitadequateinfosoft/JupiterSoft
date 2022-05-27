using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JupiterSoft.Models
{
    public enum COM_Code
    {
        one = 1, //(01 hex) 0000 0001
        two = 2, //(02 hex) 0000 0010
        three = 3, //(03 hex) 0000 0011 Read Function Code
        four = 4, //(04 hex) 0000 0100
        five = 5, //(05 hex) 0000 0101	
        six = 6, //(06 hex) 0000 0110
        fifteen = 15, //(0F hex) 0000 1111	
        sixteen = 16, //(10 hex) 0001 0000	Write Function Code
        onetwonine = 129,//(81 hex) 1000 0001 exception code for 1
        onethreezero = 130,//(82 hex) 1000 0010 exception code for 2
        onethreeone = 131, //(83 hex) 1000 0011 exception code for 3,
        onethreetwo = 132, //132 (84 hex) 1000 0100 exception code for 4
        onethreethree = 133, //133 (85 hex) 1000 0101 exception code for 5
        onethreefour = 134, //134 (86 hex) 1000 0110 exception code for 6
        onefourthree = 143, //143 (8F hex) 1000 1111 exception code for 15
        onefourfour = 144 // (90 hex) 1001 0000 exception code for 16
    }
}
