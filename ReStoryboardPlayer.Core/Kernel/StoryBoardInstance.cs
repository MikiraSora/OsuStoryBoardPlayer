﻿using ReOsuStoryBoardPlayer.Core.Base;
using ReOsuStoryBoardPlayer.Core.Commands;
using ReOsuStoryBoardPlayer.Core.Commands.Group.Trigger;
using ReOsuStoryBoardPlayer.Core.Kernel;
using ReOsuStoryBoardPlayer.Core.Parser;
using ReOsuStoryBoardPlayer.Core.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ReOsuStoryBoardPlayer.Core.Utils;

namespace ReOsuStoryBoardPlayer.Core.Kernel
{
    /// <summary>
    /// SB更新物件的核心，通过Update来更新物件
    /// </summary>
    public class StoryBoardInstance
    {
        /// <summary>
        /// 已加载的物件集合
        /// </summary>
        public LinkedList<StoryBoardObject> StoryboardObjectList { get; private set; }

        private LinkedListNode<StoryBoardObject> CurrentScanNode;

        /// <summary>
        /// 正在执行的物件集合
        /// </summary>
        public List<StoryBoardObject> UpdatingStoryboardObjects { get; private set; }

        public BeatmapFolderInfo Info { get; }

        private ParallelOptions parallel_options = new ParallelOptions() { MaxDegreeOfParallelism=Setting.UpdateThreadCount };

        public StoryBoardInstance(BeatmapFolderInfo info)
        {
            Info=info;

            StoryboardObjectList=new LinkedList<StoryBoardObject>();

            //int audioLeadIn = 0;

            #region Load and Parse osb/osu file

            using (StopwatchRun.Count("Load and Parse osb/osu file"))
            {
                List<StoryBoardObject> temp_objs_list = new List<StoryBoardObject>(), parse_osb_storyboard_objs = new List<StoryBoardObject>();
                
                //get objs from osu file
                List<StoryBoardObject> parse_osu_storyboard_objs = string.IsNullOrWhiteSpace(info.osu_file_path)?new List<StoryBoardObject>():StoryboardParserHelper.GetStoryBoardObjects(info.osu_file_path);
                AdjustZ(parse_osu_storyboard_objs, 0);

                if ((!string.IsNullOrWhiteSpace(info.osb_file_path))&&File.Exists(info.osb_file_path))
                {
                    parse_osb_storyboard_objs=StoryboardParserHelper.GetStoryBoardObjects(info.osb_file_path);
                    AdjustZ(parse_osb_storyboard_objs, 0);
                }

                temp_objs_list=CombineStoryBoardObjects(parse_osb_storyboard_objs, parse_osu_storyboard_objs);

                //delete Background object if there is a normal storyboard object which is same image file.
                var background_obj = temp_objs_list.Where(c => c is StoryboardBackgroundObject).FirstOrDefault();
                if (temp_objs_list.Any(c => c.ImageFilePath==background_obj?.ImageFilePath&&(!(c is StoryboardBackgroundObject))))
                {
                    Log.User($"Found another same background image object and delete all background objects.");
                    temp_objs_list.RemoveAll(x=>x is StoryboardBackgroundObject);
                }
                else
                {
                    if (background_obj!=null)
                        background_obj.Z=-1;
                }

                temp_objs_list.Sort((a, b) =>
                {
                    return a.FrameStartTime-b.FrameStartTime;
                });

                foreach (var obj in temp_objs_list)
                    StoryboardObjectList.AddLast(obj);

                StoryboardObjectList.AsParallel().ForAll(c => c.SortCommands());

                CurrentScanNode=StoryboardObjectList.First;
            }

            #endregion Load and Parse osb/osu file

            var limit_update_count = StoryboardObjectList.CalculateMaxUpdatingObjectsCount();

            UpdatingStoryboardObjects=new List<StoryBoardObject>(limit_update_count);

            void AdjustZ(List<StoryBoardObject> list, int base_z)
            {
                list.Sort((a, b) => (int)(a.FileLine-b.FileLine));
            }
        }

        private List<StoryBoardObject> CombineStoryBoardObjects(List<StoryBoardObject> osb_list, List<StoryBoardObject> osu_list)
        {
            List<StoryBoardObject> result = new List<StoryBoardObject>();

            Add(Layout.Background);
            Add(Layout.Fail);
            Add(Layout.Pass);
            Add(Layout.Foreground);

            int z=0;
            foreach (var obj in result)
            {
                obj.Z=z++;
            }

            return result;

            void Add(Layout layout)
            {
                result.AddRange(osu_list.Where(x => x.layout==layout));//先加osu
                result.AddRange(osb_list.Where(x=>x.layout==layout).Select(x =>
                {
                    x.FromOsbFile=true;
                    return x;
                }));//后加osb覆盖
            }
        }

        /// <summary>
        /// 重置物件的时间轴状态
        /// </summary>
        private void Flush()
        {
            UpdatingStoryboardObjects.Clear();

            CurrentScanNode=StoryboardObjectList.First;

            //重置触发器状态
            TriggerListener.DefaultListener.Reset();
        }

        private bool Scan(float current_time)
        {
            LinkedListNode<StoryBoardObject> LastAddNode = null;

            while (CurrentScanNode!=null&&CurrentScanNode.Value.FrameStartTime<=current_time/* && current_time <= CurrentScanNode.Value.FrameEndTime*/ )
            {
                var obj = CurrentScanNode.Value;

                if (current_time>obj.FrameEndTime)
                {
                    CurrentScanNode=CurrentScanNode.Next;
                    continue;
                }

                //重置物件变换初始值
                obj.ResetTransform();

                //添加到更新列表
                UpdatingStoryboardObjects.Add(obj);

                LastAddNode=CurrentScanNode;

                CurrentScanNode=CurrentScanNode.Next;
            }

            if (LastAddNode!=null)
            {
                CurrentScanNode=LastAddNode.Next;
            }

            return LastAddNode!=null;
        }

        private float prev_time = int.MinValue;

        /// <summary>
        /// 更新物件，因为是增量更新维护，所以current_time递减或变小时，必须先Flush()后Update().
        /// </summary>
        /// <param name="current_time"></param>
        public void Update(float current_time)
        {
            if (current_time<prev_time)
                Flush();
            else
                UpdatingStoryboardObjects.RemoveAll((obj) => current_time>obj.FrameEndTime||current_time<obj.FrameStartTime);

            prev_time=current_time;
            
            bool hasAdded = Scan(current_time);
            
            if (hasAdded)
            {
                UpdatingStoryboardObjects.Sort((a, b) =>
                {
                    return a.Z-b.Z;
                });
            }

            if (UpdatingStoryboardObjects.Count>=Setting.ParallelUpdateObjectsLimitCount&&Setting.ParallelUpdateObjectsLimitCount!=0)
            {
                Parallel.ForEach(UpdatingStoryboardObjects, parallel_options,obj => obj.Update(current_time));
            }
            else
            {
                foreach (var obj in UpdatingStoryboardObjects)
                    obj.Update(current_time);
            }
        }

        ~StoryBoardInstance()
        {

        }

        public override string ToString() => $"{Info.folder_path}";
    }
}