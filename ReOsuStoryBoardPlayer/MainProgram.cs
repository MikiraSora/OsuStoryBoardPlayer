using ReOsuStoryBoardPlayer.DebugTool;
using ReOsuStoryBoardPlayer.Kernel;
using ReOsuStoryBoardPlayer.Parser;
using ReOsuStoryBoardPlayer.Player;
using System;
using ReOsuStoryBoardPlayer.ProgramCommandParser;
using System.Linq;
using ReOsuStoryBoardPlayer.DebugTool.Debugger.CLIController;
using ReOsuStoryBoardPlayer.Parser.Extension;
using System.Runtime.InteropServices;
using ReOsuStoryBoardPlayer.Parser.Stream;
using ReOsuStoryBoardPlayer.Parser.Collection;
using ReOsuStoryBoardPlayer.Parser.Reader;
using System.IO;

namespace ReOsuStoryBoardPlayer
{
    public class MainProgram
    {
        public static void Main(string[] argv)
        {
            Setting.Init();

            ParseProgramCommands(argv, out var beatmap_folder);

            Setting.PrintSettings();
            
            if (Setting.MiniMode)
            {
                DebuggerManager.AddDebugger(new CLIControllerDebugger());
            }
            else
            {
#if DEBUG
                DebuggerHelper.SetupDebugEnvironment();
#else
                DebuggerHelper.SetupReleaseEnvironment();
#endif
            }

            var info = BeatmapFolderInfo.Parse(beatmap_folder);

            //init audio
            var player = new MusicPlayer();
            player.Load(info.audio_file_path);

            MusicPlayerManager.ApplyPlayer(player);

            //load storyboard objects
            var instance = new StoryBoardInstance(info);

            //init window
            StoryboardWindow window = new StoryboardWindow(Setting.Width, Setting.Height);
            window.LoadStoryboardInstance(instance);
            //init control panel and debuggers

            #region CLI Control

            //Log.AbleDebugLog = false;

            #endregion

            player.Play();
            window.Run();
        }

        private static void ParseProgramCommands(string[] argv, out string beatmap_folder)
        {
            beatmap_folder=@"G:\SBTest\511637 solfa featChata - I will";

            var sb = new ArgParser(new ParamParserV2('-', '\"', '\''));
            var args = sb.Parse(argv);

            if (args!=null)
            {
                if (args.Switches.Any(k => k=="help"))
                {
                    Console.WriteLine("please visit here: https://github.com/MikiraSora/OsuStoryBoardPlayer/wiki/Program-command-options");
                    Exit("");
                }

                if (args.FreeArgs!=null)
                    beatmap_folder=args.FreeArgs.FirstOrDefault()??beatmap_folder;

                if (args.TryGetArg(out var valW, "width", "w"))
                    Setting.Width=int.Parse(valW);

                if (args.TryGetArg(out var draw_count, "multi_instance_render", "mtr"))
                    Setting.DrawCallInstanceCountMax=int.Parse(draw_count);

                if (args.TryGetArg(out var valH, "height", "h"))
                    Setting.Height=int.Parse(valH);

                if (args.TryGetArg(out var folder, "folder", "f"))
                    beatmap_folder=folder;

                if (args.TryGetArg(out var p_update_limit, "parallel_update_limit", "pu"))
                    Setting.ParallelUpdateObjectsLimitCount=p_update_limit.ToInt();

                if (args.TryGetArg(out var update_thread_count, "update_thread_count", "ut"))
                    Setting.UpdateThreadCount = update_thread_count.ToInt();

                if (args.TryGetArg(out var max_fps, "fps"))
                    Setting.MaxFPS = max_fps.ToInt();

                Setting.EnableTimestamp=args.Switches.Any(k => k=="enable_timestamp");
                Setting.EnableLoopCommandExpand=args.Switches.Any(k => k=="enable_loop_expand");
                Setting.MiniMode=args.Switches.Any(k => k=="mini");
                Setting.EnableSplitMoveScaleCommand=!args.Switches.Any(k => k=="disable_split");
                Setting.FunReverseEasing=args.Switches.Any(k => k=="fun_reverse_easing");
                Setting.EnableRuntimeOptimzeObjects=!args.Switches.Any(k => k=="disable_runtime_optimze");
                Setting.EnableHighPrecisionFPSLimit = !args.Switches.Any(k => k == "disable_hp_fps_limit");

                //额外功能
                if (args.TryGetArg(out var parse_type, "parse"))
                {
                    var parse_osb = parse_type=="osb";
                    args.TryGetArg(out var output_path, "parse_output");
                    SerializeDecodeStoryboardContent(beatmap_folder, parse_osb, output_path);
                }
            }
        }

        #region ProgramCommands

        //将解析好的内容再序列化成osb文件格式的文本内容
        private static void SerializeDecodeStoryboardContent(string beatmap_folder,bool parse_osb,string output_path)
        {
            try
            {
                var info = BeatmapFolderInfo.Parse(beatmap_folder);
                var input_file = parse_osb ? info.osb_file_path : info.osu_file_path;
                output_path=string.IsNullOrWhiteSpace(output_path) ? input_file+".parse_output" : output_path;

                Log.User($"Start serialize {input_file} ....");
                using (var writer = new StreamWriter(File.OpenWrite(output_path)))
                {
                    writer.WriteLine($"[{Section.Events.ToString()}]");

                    OsuFileReader reader = new OsuFileReader(input_file);

                    VariableCollection collection = new VariableCollection(new VariableReader(reader).EnumValues());

                    SectionReader section = new SectionReader(Section.Events, reader);

                    EventReader event_section = new EventReader(reader, collection);

                    foreach (var line in section.EnumValues())
                    {
                        var decode_line = event_section.LineProcessVariable(line);
                        writer.WriteLine(decode_line);
                    }
                }

                Log.User("Serialize successfully! it output to "+output_path);
                Exit("Serialize successfully! it output to "+output_path);
            }
            catch (Exception e)
            {
                Log.Error("Serialize failed!"+e.Message);
                Exit("Serialize failed!"+e.Message);
            }
        }

        #endregion

        private static void Exit(string reason)
        {
            if ((!Setting.MiniMode)&&(!string.IsNullOrWhiteSpace(reason)))
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