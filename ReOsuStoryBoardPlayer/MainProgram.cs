using ReOsuStoryBoardPlayer.DebugTool;
using ReOsuStoryBoardPlayer.DebugTool.Debugger.ControlPanel;
using ReOsuStoryBoardPlayer.DebugTool.Debugger.ObjectInfoVisualizer;
using ReOsuStoryBoardPlayer.Kernel;
using ReOsuStoryBoardPlayer.Parser;
using ReOsuStoryBoardPlayer.Player;
using System;
using System.IO;
using ReOsuStoryBoardPlayer.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer
{
    public class MainProgram
    {
        public static void Main(string[] argv)
        {
            string beatmap_folder = @"G:\SBTest\237977 marina - Towa yori Towa ni";
            int w = (int)(854), h = (int)(480);
            var sb = new ArgParser(new ParamParserV2('-', '\"', '\''));
            var args = sb.Parse(argv);
            if (args != null)
            {
                if (args.FreeArgs != null)
                    beatmap_folder = args.FreeArgs.First();
                if (args.Switches.Any(k => k == "mini"))
                {
                    Log.MiniMode = true;
                    if (args.TryGetArg("w", out var valW))
                        w = int.Parse(valW);
                    if (args.TryGetArg("h", out var valH))
                        w = int.Parse(valH);
                }
            }

            var info = BeatmapFolderInfo.Parse(beatmap_folder);

            //init audio
            var player = new MusicPlayer();
            player.Load(info.audio_file_path);

            MusicPlayerManager.ApplyPlayer(player);

            //load storyboard objects
            var instance = new StoryBoardInstance(info);

            //init window
            StoryboardWindow window = new StoryboardWindow(w, h);
            window.LoadStoryboardInstance(instance);
            //init control panel and debuggers



            #region CLI Control

            //Log.AbleDebugLog = false;
            if (Log.MiniMode)
            {
                Task.Run(() =>
                {
                    bool someContition = true; //一些逻辑
                    while (someContition)
                    {
                        //加载时输出 Loading
                        //加载完输出load finished这样
                        while (true)
                        {
                            Console.WriteLine(MusicPlayerManager.ActivityPlayer.CurrentTime);
                            Thread.Sleep(100);
                        }
                        //播完完输出finished这样
                    }
                });
                Task.Run(() =>
                {
                    while (true)
                    {
                        string input = Console.ReadLine();
                        if (string.IsNullOrEmpty(input))
                            continue;
                        var cmd = new CommandParser(new ParamParserV2('-', '\"', '\'')).Parse(input, out var cmdName);
                        switch (cmdName)
                        {
                            case "file":
                                //load a file
                                break;
                            case "play":
                                MusicPlayerManager.ActivityPlayer.Play();
                                break;
                            case "pause":
                                MusicPlayerManager.ActivityPlayer.Pause();
                                break;
                            case "jump":
                                var str = cmd.FreeArgs.FirstOrDefault();
                                if (str==null) break;
                                var num = uint.Parse(str);
                                MusicPlayerManager.ActivityPlayer.Jump(num);
                                break;
                            case "exit":
                            case "quit":
                                window.Close();
                                break;
                            case "moveTo": //x,y坐标
                                throw new NotImplementedException();
                            case "scale": //1.0为基准这样
                                //case "sizeTo": //或者具体到分辨率
                                throw new NotImplementedException();
                            default:
                                break;
                        }
                    }
                });
            }

            #endregion

            player.Play();
            window.Run();
        }

        private static void Exit(string reason)
        {
            if (!Log.MiniMode)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(reason);
                Console.ResetColor();
                Console.ReadKey();
            }
            Environment.Exit(0);
        }
    }
}