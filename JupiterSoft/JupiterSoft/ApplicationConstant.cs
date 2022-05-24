using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JupiterSoft
{
   public enum ElementConstant
    {
        // Move Enum
        Ten_Steps_Move,
        Turn_Fiften_Degree_Right_Move,
        Turn_Fiften_Degree_Left_Move,
        Pointer_State_Move,
        Rotation_Style_Move,
        X_Postition_Move,
        Y_Position_Move,

        //Events Enum
        Start_Event,
        Clicked_Event,
        Connect_Event,
        Disconnect_Event,
        Send_Message_Event,
        Space_Key_pressed_Event,
        Receive_Message_Event,
        BroadCast_Message_Event,
        BroadCast_Message_Wait_Event,

        //Control Enum
        Wait_One_Second_Control,
        Repeat_Ten_Control,
        Forever_Control,
        If_Then_Control,
        If_Then_Else_Control,
        Wait_Until_Control,
        Repeat_Until_Control,
        Stop_All_Control,

        //Sensing Enum
        Touching_Mouse_Pointer_Sensor,
        Ask_And_Wait_Sensor,
        Answer_Sensor,
        Timer_Sensor,
        Reset_Timer_Sensor,
        Device_Name_Sensor,


        //Operator Enum,
        Add_Operator,
        Subtract_Operator,
        Multiply_Operator,
        Devide_Operator,
        Greater_Then_Operator,
        Lesser_Then_Operator,
        Equal_Operator,
        And_Gate_Operator,
        Or_Gate_Operator,
        Not_Operator,
        Round_Operator,

        //Look Enum
        Hello_2s_Look,
        Hello_Look,
        Change_Size_By10_Look,
        Set_Size_To_100P_Look,
        Color_Effect_By25_Look,
        Set_Color_Effect_To0_Look,
        Clear_Graphic_Effect_Look,
        Show_Look,
        Hide_Look,
        Costume_Number_Look,
        Backdrop_Number_Look,
        Size_Look,


        //Sound Enum,
        Play_Sound_Until_Done_Sound,
        Start_Sound,
        Stop_All_Sound,
        Clear_Sound_Effect_Sound,
        Change_Volume_by_Number_Sound,
        Set_Volume_To_Percent_Sound,
        Volume_Sound

    }
}
