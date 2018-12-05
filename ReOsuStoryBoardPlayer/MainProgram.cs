﻿using ReOsuStoryBoardPlayer.DebugTool;
using ReOsuStoryBoardPlayer.DebugTool.Debugger.ControlPanel;
using ReOsuStoryBoardPlayer.DebugTool.Debugger.ObjectInfoVisualizer;
using ReOsuStoryBoardPlayer.Kernel;
using ReOsuStoryBoardPlayer.Parser;
using ReOsuStoryBoardPlayer.Player;
using System;
using System.IO;

namespace ReOsuStoryBoardPlayer
{
    public class MainProgram
    {
        public static void Main(string[] argv)
        {
#if DEBUG
            Log.AbleDebugLog = true;
#else
            Log.AbleDebugLog = false;
#endif

            string beatmap_folder = string.Empty;

            if (argv.Length==0)
            {
                beatmap_folder=@"G:\SBTest\839266 Jeremy Blake - Flex";
            }
            else if (string.IsNullOrWhiteSpace(argv[0]))
                Exit("Please drag beatmap folder to this program.");
            else
                beatmap_folder=argv[0];

            //get folder info
            var info =BeatmapFolderInfo.Parse(beatmap_folder);

            //init audio
            var player = new MusicPlayer();
            player.Load(info.audio_file_path);

            MusicPlayerManager.ApplyPlayer(player);

            //load storyboard objects
            var instance= new StoryBoardInstance(info);

            //init window
            StoryboardWindow window = new StoryboardWindow(1280, 720);
            window.LoadStoryboardInstance(instance);

            //init control panel and debuggers
#if DEBUG
            DebuggerHelper.SetupDebugEnvironment();
#else
            DebuggerHelper.SetupReleaseEnvironment();
#endif
            player.Play();
            window.Run();
        }

        private static void Exit(string reason)
        {
            Console.BackgroundColor = ConsoleColor.Red;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(reason);
            Console.ResetColor();
            Console.ReadKey();
            Environment.Exit(0);
        }
    }
}