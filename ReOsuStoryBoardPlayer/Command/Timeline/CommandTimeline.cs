﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ReOsuStoryBoardPlayer.Commands
{
    public class CommandTimeline : List<Command>
    {
        public int StartTime;
        public int EndTime;

        public new void Add(Command command)
        {
            base.Add(command);

            //update StartTime/EndTime
            StartTime=Math.Min(StartTime, command.StartTime);
            EndTime=Math.Max(EndTime, command.StartTime);
        }

        private Command selected_command;

        public Command PickCommand(float current_time)
        {
            Debug.Assert(!float.IsNaN(current_time), $"current_time is not a number");

            Command command = null;

            //cache
            if (selected_command != null && (selected_command.StartTime <= current_time && current_time <= selected_command.EndTime))
                return selected_command;

            if (current_time < this.First().StartTime)
                return selected_command = this.First();
            else if (current_time > this.Last().EndTime)
                return selected_command = this.Last();

            //尝试选取在时间范围内的命令
            if (command == null)
                foreach (var cmd in this)
                    if (current_time >= cmd.StartTime && current_time <= cmd.EndTime)
                        return selected_command = cmd;

            //尝试选取在命令之间的前者
            if (command == null)
                for (int i = 0; i < Count - 1; i++)
                {
                    var cur_cmd = this[i];
                    var next_cmd = this[i + 1];

                    if (current_time >= cur_cmd.EndTime && current_time <= next_cmd.StartTime)
                    {
                        return selected_command = cur_cmd;
                    }
                }

            return selected_command = null;
        }
    }
}