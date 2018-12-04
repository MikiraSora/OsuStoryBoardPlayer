﻿using ReOsuStoryBoardPlayer.DebugTool.Debugger.ControlPanel;
using ReOsuStoryBoardPlayer.DebugTool.Debugger.ObjectInfoVisualizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.DebugTool
{
    public static class DebuggerHelper
    {
        public static void SetupDebugEnvironment()
        {
            DebuggerManager.AddDebugger(new ControlPanelDebugger());
            DebuggerManager.AddDebugger(new ObjectVisualizerDebugger());
        }

        public static void SetupReleaseEnvironment()
        {
            //提供个控制器
            DebuggerManager.AddDebugger(new ControlPanelDebugger());
        }
    }
}