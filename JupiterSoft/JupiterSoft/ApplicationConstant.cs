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
        Connect_Motor_Event,
        Disconnect_Motor_Event,
        Connect_Weight_Event,
        Disconnect_Weight_Event,
        Connect_Camera_Event,
        Disconnect_Camera_Event,
        Connect_ControlCard_Event,
        Disconnect_ControlCard_Event,
        Start_Stream_Camera_Event,
        Stop_Stream_Camera_Event,
        Start_Camera_Recording_Event,
        Stop_Camera_recording_Event,

        //Control Enum
        Delay,
        Repeat_Ten_Control,
        Repeat_Control,
        Stop_Repeat,
        Forever_Control_Start,
        If_Condition_Start,
        Else_If_Start,
        Else_Start,
        End_Scope,
        Wait_Until_Control,
        Repeat_Until_Control,
        Stop_All_Control,

        //Functions Enum
        Read_Motor_Frequency,
        Change_Motor_Frequency,
        Turn_ON_Motor,
        Turn_OFF_Motor,
        Read_All_Card_In_Out,
        Write_Card_Out,
        Read_Weight,


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

    public enum ConditionConstant
    {
        contains,
        does_not_contains,
        is_equal_to,
        is_not_equal_to,
        is_greater_then,
        is_greater_then_or_equal_to,
        is_less_then,
        is_less_then_or_equal_to,
        starts_with,
        does_not_start_with,
        ends_with,
        does_not_ends_with
    }
}
