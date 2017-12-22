﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer
{
    public static class CommandExecutor
    {
        public delegate void CommandFunc(StoryBoardObject ref_obj, float interpolator_value, Command command);

        #region Build CommandFunctionMap

        public static Dictionary<Event, CommandFunc> CommandFunctionMap = new Dictionary<Event, CommandFunc>()
        {
            {Event.Move,Move},
            {Event.Fade,Fade},
            {Event.Color,Color},
            {Event.MoveX,MoveX},
            {Event.MoveY,MoveY},
            {Event.Scale,Scale},
            {Event.VectorScale,ScaleVector},
            {Event.Rotate,Rotate},
            {Event.Loop,Loop},
            {Event.HorizonFlip,HorizonFlip},
            {Event.VerticalFlip,VerticalFlip},
            {Event.AdditiveBlend,AdditiveBlend}
        };

        #endregion

        #region Command Implatements

        public static void Move(StoryBoardObject ref_obj, float current_value, Command command)
        {
            MoveCommandParameters parameters = (MoveCommandParameters)command.Parameters;

            Vector cal_result = current_value >= 1 ? parameters.EndPosition : (current_value <= 0 ? parameters.StartPostion : parameters.StartPostion + parameters.Distance * current_value);
    
            ref_obj.Postion = cal_result;
        }

        public static void MoveX(StoryBoardObject ref_obj, float current_value, ReOsuStoryBoardPlayer.Command command)
        {
            MoveXCommandParameters parameters = (MoveXCommandParameters)command.Parameters;

            float cal_result = current_value >= 1 ? parameters.EndX : (current_value <= 0 ? parameters.StartX : parameters.StartX + parameters.Distance * current_value);

            ref_obj.Postion.x = cal_result;
        }

        public static void MoveY(StoryBoardObject ref_obj, float current_value, ReOsuStoryBoardPlayer.Command command)
        {
            MoveYCommandParameters parameters = (MoveYCommandParameters)command.Parameters;

            float cal_result = current_value >= 1 ? parameters.EndY : (current_value <= 0 ? parameters.StartY : parameters.StartY + parameters.Distance * current_value);

            ref_obj.Postion.y = cal_result;
        }

        public static void Rotate(StoryBoardObject ref_obj, float current_value, ReOsuStoryBoardPlayer.Command command)
        {
            RotateCommandParamesters parameters = (RotateCommandParamesters)command.Parameters;

            float cal_result = 0;

            int sign = Math.Sign(parameters.Distance);

            if (current_value<=0)
            {
                cal_result = Math.Abs(parameters.StartRotate);
            }
            else if (current_value>=1)
            {
                cal_result = Math.Abs(parameters.EndRotate);
            }
            else
            {
                cal_result = parameters.StartRotate + sign * parameters.Distance * current_value;
            }

            cal_result = (float)(cal_result/Math.PI*180.0f);

#if DEBUG
            //todo this is a bug fix
            if (float.IsNaN(cal_result))
            {
                cal_result = 0;
            }
#endif
            ref_obj.Rotate =cal_result ;
        }

        public static void Fade(StoryBoardObject ref_obj, float current_value, ReOsuStoryBoardPlayer.Command command)
        {
            FadeCommandParamesters parameters = (FadeCommandParamesters)command.Parameters;
            
            float cal_result = current_value >= 1 ? parameters.EndFade : (current_value <= 0 ? parameters.StartFade : parameters.StartFade + parameters.Distance * current_value);
            
            ref_obj.Color.w = cal_result;
        }

        public static void Scale(StoryBoardObject ref_obj, float current_value, ReOsuStoryBoardPlayer.Command command)
        {
            ScaleCommandParameters parameters = (ScaleCommandParameters)command.Parameters;

            float cal_result = current_value >= 1 ? parameters.EndScale : (current_value <= 0 ? parameters.StartScale : parameters.StartScale + parameters.Distance * current_value);

            ref_obj.Scale.x=ref_obj.Scale.y= cal_result;
        }

        public static void ScaleVector(StoryBoardObject ref_obj, float current_value, ReOsuStoryBoardPlayer.Command command)
        {
            ScaleVectorCommandParamesters parameters = (ScaleVectorCommandParamesters)command.Parameters;

            Vector cal_result = current_value >= 1 ? parameters.EndScale : (current_value <= 0 ? parameters.StartScale : parameters.StartScale + parameters.Distance * current_value);

            ref_obj.Scale = cal_result;
        }

        public static void Color(StoryBoardObject ref_obj, float current_value, ReOsuStoryBoardPlayer.Command command)
        {
            ColorCommandParameters parameters = (ColorCommandParameters)command.Parameters;

            Vec4 temp = new Vec4();
            temp.x = Math.Min((parameters.StartColor.x + parameters.Distance.x * current_value),1);
            temp.y = Math.Min((parameters.StartColor.y + parameters.Distance.y * current_value),1);
            temp.z = Math.Min((parameters.StartColor.z + parameters.Distance.z * current_value),1);

            Vec4 cal_result = current_value >= 1 ? parameters.EndColor : (current_value <= 0 ? parameters.StartColor : temp);
                
            float a = ref_obj.Color.w;
            ref_obj.Color = temp;
            ref_obj.Color.w = a;
        }

        public static void Loop(StoryBoardObject ref_obj, float current_value, ReOsuStoryBoardPlayer.Command _command)
        {
            int recovery_time = (int)((_command.EndTime - _command.StartTime) * current_value);
            LoopCommand loop_command = (LoopCommand)_command;

            int current_time = (int)(recovery_time % loop_command.LoopParamesters.CostTime);

            var command_list = loop_command.LoopParamesters.LoopCommandList;

            Command command = null;
            if (current_time < command_list[0].StartTime)
            {
                //早于开始前
                command = command_list[0];
            }
            else if (current_time > command_list[command_list.Count - 1].EndTime)
            {
                //迟于结束后
                command = command_list[command_list.Count - 1];
            }

            if (command == null)
            {
                foreach (var cmd in command_list)
                {
                    if (current_time >= cmd.StartTime && current_time <= cmd.EndTime)
                    {
                        command = cmd;
                        break;
                    }
                }
            }

            if (command == null)
            {
                for (int i = 0; i < command_list.Count - 1; i++)
                {
                    var cur_cmd = command_list[i];
                    var next_cmd = command_list[i + 1];

                    if (current_time >= cur_cmd.EndTime && current_time <= next_cmd.StartTime)
                    {
                        command = cur_cmd;
                        break;
                    }
                }
            }

            if (command != null)
            {
                CommandExecutor.DispatchCommandExecute(ref_obj, current_time, command);
            }

        }

        public static void Parameter(StoryBoardObject ref_obj, float current_value, Command command)
        {
            ParameterCommandParamester param = (ParameterCommandParamester)command.Parameters;

            switch (param.Effect)
            {
                case EffectParameter.HorizontalFlip:
                    break;
                case EffectParameter.VerticalFlip:
                    break;
                case EffectParameter.AdditiveBlend:
                    ref_obj.IsAdditive = (current_value >= 0 && current_value <= 1);
                    break;
                default:
                    break;
            }
        }

        public static void AdditiveBlend(StoryBoardObject ref_obj, float current_value, Command command)
        {
            ref_obj.IsAdditive = current_value <= 1;
        }

        public static void HorizonFlip(StoryBoardObject ref_obj, float current_value, Command command)
        {
            ref_obj.IsHorizonFlip = current_value <= 1;
        }

        public static void VerticalFlip(StoryBoardObject ref_obj, float current_value, Command command)
        {
            ref_obj.IsVerticalFlip = current_value <= 1;
        }

        #endregion

        static Command[] _ExecutedCommandRegisterArray = new Command[14];

        public static void ClearCommandRegisterArray() => Array.Clear(_ExecutedCommandRegisterArray, 0, 14);

        public static void DispatchCommandExecute(StoryBoardObject ref_obj, float current_playing_time, Command command)
        {

            #region Check Command Conflct

            var reg_cmd = _ExecutedCommandRegisterArray[(int)command.CommandEventType];
            if (reg_cmd != command&& reg_cmd!=null&& command.EndTime < reg_cmd.EndTime)
            {
                    return;
            }

            _ExecutedCommandRegisterArray[(int)command.CommandEventType] = command;

            #endregion

            #region Calculate interpolator value

            float current_value = command.Easing.calculate(current_playing_time- command.StartTime, command.StartTime, command.EndTime);

            //fix infinity
            if (float.IsInfinity(current_value))
            {
                current_value = 1;
            }

            #endregion

            command.executor(ref_obj, current_value, command);
        }
    }
}
