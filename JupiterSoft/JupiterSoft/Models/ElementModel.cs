using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JupiterSoft.Models
{
    public class ElementModel
    {
        // Move
        public int Ten_Steps_Move { get; set; }
        public int Turn_Fiften_Degree_Right_Move { get; set; }
        public int Turn_Fiften_Degree_Left_Move { get; set; }
        public int Pointer_State_Move { get; set; }
        public int Rotation_Style_Move { get; set; }
        public int X_Postition_Move { get; set; }
        public int Y_Position_Move { get; set; }

        //Events
        public int Start_Event { get; set; }
        public int Connect_Motor_Event { get; set; }
        public int Disconnect_Motor_Event { get; set; }
        public int Connect_Weight_Event { get; set; }
        public int Disconnect_Weight_Event { get; set; }
        public int Connect_Camera_Event { get; set; }
        public int Disconnect_Camera_Event { get; set; }
        public int Connect_ControlCard_Event { get; set; }
        public int Disconnect_ControlCard_Event { get; set; }
        public int Receive_Message_Event { get; set; }
        public int Start_Stream_Camera_Event { get; set; }
        public int Stop_Stream_Camera_Event { get; set; }
        public int Start_Camera_Recording_Event { get; set; }
        public int Stop_Camera_recording_Event { get; set; }

        //Control
        public int Wait_One_Second_Control { get; set; }
        public int Repeat_Ten_Control { get; set; }
        public int Forever_Control_Start { get; set; }
        public int If_Condition_Start { get; set; }
        public int Else_If_Start { get; set; }
        public int Else_Start { get; set; }
        public int End_Scope { get; set; }
        public int Wait_Until_Control { get; set; }
        public int Repeat_Until_Control { get; set; }
        public int Stop_All_Control { get; set; }

        //Sensor
        public int Touching_Mouse_Pointer_Sensor { get; set; }
        public int Ask_And_Wait_Sensor { get; set; }
        public int Answer_Sensor { get; set; }
        public int Timer_Sensor { get; set; }
        public int Reset_Timer_Sensor { get; set; }
        public int Device_Name_Sensor { get; set; }

        //Operator
        public int Add_Operator { get; set; }
        public int Subtract_Operator { get; set; }
        public int Multiply_Operator { get; set; }
        public int Devide_Operator { get; set; }
        public int Greater_Then_Operator { get; set; }
        public int Lesser_Then_Operator { get; set; }
        public int Equal_Operator { get; set; }
        public int And_Gate_Operator { get; set; }
        public int Or_Gate_Operator { get; set; }
        public int Not_Operator { get; set; }
        public int Round_Operator { get; set; }

        //Look
        public int Hello_2s_Look { get; set; }
        public int Hello_Look { get; set; }
        public int Change_Size_By10_Look { get; set; }
        public int Set_Size_To_100P_Look { get; set; }
        public int Color_Effect_By25_Look { get; set; }
        public int Set_Color_Effect_To0_Look { get; set; }
        public int Clear_Graphic_Effect_Look { get; set; }
        public int Show_Look { get; set; }
        public int Hide_Look { get; set; }
        public int Costume_Number_Look { get; set; }

        public int Backdrop_Number_Look { get; set; }
        public int Size_Look { get; set; }

        //Sound
        public int Play_Sound_Until_Done_Sound { get; set; }
        public int Start_Sound { get; set; }
        public int Stop_All_Sound { get; set; }
        public int Clear_Sound_Effect_Sound { get; set; }
        public int Change_Volume_by_Number_Sound { get; set; }
        public int Set_Volume_To_Percent_Sound { get; set; }
        public int Volume_Sound { get; set; }
    }

    public class ConditionModel
    {
        public int value { get; set; }
        public string text { get; set; }
    }

    public static class ElementOp
    {
        public static ElementModel GetElementModel()
        {
            ElementModel elementOp = new ElementModel
            {
                // Move Enum
                Ten_Steps_Move = (int)ElementConstant.Ten_Steps_Move,
                Turn_Fiften_Degree_Right_Move = (int)ElementConstant.Turn_Fiften_Degree_Right_Move,
                Turn_Fiften_Degree_Left_Move = (int)ElementConstant.Turn_Fiften_Degree_Left_Move,
                Pointer_State_Move = (int)ElementConstant.Pointer_State_Move,
                Rotation_Style_Move = (int)ElementConstant.Rotation_Style_Move,
                X_Postition_Move = (int)ElementConstant.X_Postition_Move,
                Y_Position_Move= (int)ElementConstant.Y_Position_Move,

                //Events Enum
                Start_Event=(int)ElementConstant.Start_Event,
                Connect_Motor_Event = (int)ElementConstant.Connect_Motor_Event,
                Disconnect_Motor_Event = (int)ElementConstant.Disconnect_Motor_Event,
                Connect_Weight_Event = (int)ElementConstant.Connect_Weight_Event,
                Disconnect_Weight_Event = (int)ElementConstant.Disconnect_Weight_Event,
                Connect_Camera_Event = (int)ElementConstant.Connect_Camera_Event,
                Disconnect_Camera_Event = (int)ElementConstant.Disconnect_Camera_Event,
                Connect_ControlCard_Event = (int)ElementConstant.Connect_ControlCard_Event,
                Disconnect_ControlCard_Event = (int)ElementConstant.Disconnect_ControlCard_Event,
                Receive_Message_Event = (int)ElementConstant.Receive_Message_Event,
                Start_Stream_Camera_Event = (int)ElementConstant.Start_Stream_Camera_Event,
                Stop_Stream_Camera_Event=(int)ElementConstant.Stop_Stream_Camera_Event,
                Start_Camera_Recording_Event=(int)ElementConstant.Start_Camera_Recording_Event,
                Stop_Camera_recording_Event=(int)ElementConstant.Stop_Camera_recording_Event,


                //Control Enum
                Wait_One_Second_Control = (int)ElementConstant.Wait_One_Second_Control,
                Repeat_Ten_Control = (int)ElementConstant.Repeat_Ten_Control,
                Forever_Control_Start = (int)ElementConstant.Forever_Control_Start,
                If_Condition_Start = (int)ElementConstant.If_Condition_Start,
                Else_If_Start= (int)ElementConstant.Else_If_Start,
                Else_Start = (int)ElementConstant.Else_Start,
                End_Scope = (int)ElementConstant.End_Scope,
                Wait_Until_Control = (int)ElementConstant.Wait_Until_Control,
                Repeat_Until_Control = (int)ElementConstant.Repeat_Until_Control,
                Stop_All_Control = (int)ElementConstant.Stop_All_Control,

                //Sensing Enum
                Touching_Mouse_Pointer_Sensor = (int)ElementConstant.Touching_Mouse_Pointer_Sensor,
                Ask_And_Wait_Sensor = (int)ElementConstant.Ask_And_Wait_Sensor,
                Answer_Sensor = (int)ElementConstant.Answer_Sensor,
                Timer_Sensor = (int)ElementConstant.Timer_Sensor,
                Reset_Timer_Sensor = (int)ElementConstant.Reset_Timer_Sensor,
                Device_Name_Sensor = (int)ElementConstant.Device_Name_Sensor,


                //Operator Enum,
                Add_Operator = (int)ElementConstant.Add_Operator,
                Subtract_Operator = (int)ElementConstant.Subtract_Operator,
                Multiply_Operator = (int)ElementConstant.Multiply_Operator,
                Devide_Operator = (int)ElementConstant.Devide_Operator,
                Greater_Then_Operator = (int)ElementConstant.Greater_Then_Operator,
                Lesser_Then_Operator = (int)ElementConstant.Lesser_Then_Operator,
                Equal_Operator = (int)ElementConstant.Equal_Operator,
                And_Gate_Operator = (int)ElementConstant.And_Gate_Operator,
                Or_Gate_Operator = (int)ElementConstant.Or_Gate_Operator,
                Not_Operator = (int)ElementConstant.Not_Operator,
                Round_Operator = (int)ElementConstant.Round_Operator,

                //Look Enum
                Hello_2s_Look = (int)ElementConstant.Hello_2s_Look,
                Hello_Look = (int)ElementConstant.Hello_Look,
                Change_Size_By10_Look = (int)ElementConstant.Change_Size_By10_Look,
                Set_Size_To_100P_Look = (int)ElementConstant.Set_Size_To_100P_Look,
                Color_Effect_By25_Look = (int)ElementConstant.Color_Effect_By25_Look,
                Set_Color_Effect_To0_Look = (int)ElementConstant.Set_Color_Effect_To0_Look,
                Clear_Graphic_Effect_Look = (int)ElementConstant.Clear_Graphic_Effect_Look,
                Show_Look = (int)ElementConstant.Show_Look,
                Hide_Look = (int)ElementConstant.Hide_Look,
                Costume_Number_Look = (int)ElementConstant.Costume_Number_Look,
                Backdrop_Number_Look = (int)ElementConstant.Backdrop_Number_Look,
                Size_Look = (int)ElementConstant.Size_Look,


                //Sound Enum,
                Play_Sound_Until_Done_Sound = (int)ElementConstant.Play_Sound_Until_Done_Sound,
                Start_Sound = (int)ElementConstant.Start_Sound,
                Stop_All_Sound = (int)ElementConstant.Stop_All_Sound,
                Clear_Sound_Effect_Sound = (int)ElementConstant.Clear_Sound_Effect_Sound,
                Change_Volume_by_Number_Sound = (int)ElementConstant.Change_Volume_by_Number_Sound,
                Set_Volume_To_Percent_Sound = (int)ElementConstant.Set_Volume_To_Percent_Sound,
                Volume_Sound = (int)ElementConstant.Volume_Sound
            };
            return elementOp;
        }
    }

    public static class ControlOp
    {
        public static List<ConditionModel> GetConditions()
        {
            List<ConditionModel> conditions = new List<ConditionModel>();
            conditions.Add(new ConditionModel {value=(int)ConditionConstant.contains,text="contains" });
            conditions.Add(new ConditionModel { value = (int)ConditionConstant.does_not_contains, text = "does not contains" });
            conditions.Add(new ConditionModel { value = (int)ConditionConstant.is_equal_to, text = "is equal to" });
            conditions.Add(new ConditionModel { value = (int)ConditionConstant.is_not_equal_to, text = "is not equal to" });
            conditions.Add(new ConditionModel { value = (int)ConditionConstant.is_greater_then, text = "is greater then" });
            conditions.Add(new ConditionModel { value = (int)ConditionConstant.is_greater_then_or_equal_to, text = "is greater then or equal to" });
            conditions.Add(new ConditionModel { value = (int)ConditionConstant.is_less_then, text = "is less then" });
            conditions.Add(new ConditionModel { value = (int)ConditionConstant.is_less_then_or_equal_to, text = "is less then or equal to" });
            conditions.Add(new ConditionModel { value = (int)ConditionConstant.starts_with, text = "starts with" });
            conditions.Add(new ConditionModel { value = (int)ConditionConstant.does_not_start_with, text = "does not start with" });
            conditions.Add(new ConditionModel { value = (int)ConditionConstant.ends_with, text = "ends with" });
            conditions.Add(new ConditionModel { value = (int)ConditionConstant.does_not_ends_with, text = "does not ends with" });

            return conditions;

        }
    }

}
