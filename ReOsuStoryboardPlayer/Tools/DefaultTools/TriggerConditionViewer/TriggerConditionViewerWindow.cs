﻿using ReOsuStoryboardPlayer.Core.Base;
using ReOsuStoryboardPlayer.Core.Commands.Group.Trigger;
using ReOsuStoryboardPlayer.Core.Commands.Group.Trigger.TriggerCondition;
using ReOsuStoryboardPlayer.Tools.DefaultTools.AutoTriggerContoller;
using ReOsuStoryboardPlayer.Tools.DefaultTools.ObjectInfoVisualizer;
using ReOsuStoryboardPlayer.Kernel;
using ReOsuStoryboardPlayer.Player;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace ReOsuStoryboardPlayer.Tools.DefaultTools.TriggerConditionViewer
{
    public partial class TriggerConditionViewerWindow : Form
    {
        public TriggerConditionViewerWindow()
        {
            InitializeComponent();
        }

        internal void Reset()
        {
            Clear();

            var instance = StoryboardInstanceManager.ActivityInstance;

            if (instance==null)
                return;

            var list = instance.Updater.StoryboardObjectList.Where(x => x.ContainTrigger).ToArray();

            if (list.Length!=0)
            {
                Show();
                comboBox1.Items.AddRange(list);
                comboBox1.SelectedItem=list.First();
            }
            else
            {
                Hide();
            }
        }

        private void Clear()
        {
            comboBox1.Items.Clear();
            comboBox2.Items.Clear();
            listBox1.Items.Clear();
        }

        //选择物件
        private void comboBox1_SelectedValueChanged(object sender, EventArgs e)
        {
            if (!(comboBox1.SelectedItem is StoryboardObject obj)||!obj.CommandMap.TryGetValue(Event.Trigger, out var triggers))
                return;

            comboBox2.Items.Clear();

            if (triggers.Count!=0)
            {
                comboBox2.Items.AddRange(triggers.ToArray());
                comboBox2.SelectedItem=triggers.First();
                //comboBox2_SelectedValueChanged(null, null);
            }
        }

        //选择命令
        private void comboBox2_SelectedValueChanged(object sender, EventArgs e)
        {
            var command = comboBox2.SelectedItem as TriggerCommand;
            var condition = command?.Condition as HitSoundTriggerCondition;

            var hitsounds = ToolManager.GetTool<AutoTrigger>()?.HitSoundInfos;

            if (!(comboBox1.SelectedItem is StoryboardObject obj)||hitsounds==null||hitsounds.Count==0||condition==null)
                return;

            var result = hitsounds.Where(x => condition.CheckCondition(x)&&command.CheckTimeVaild((float)x.Time)).ToArray();

            listBox1.Items.Clear();
            foreach (var item in result)
                listBox1.Items.Add(item);
        }

        private void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var hitsound = listBox1.SelectedItem as HitSoundInfo?;

            if (hitsound!=null&&MusicPlayerManager.ActivityPlayer is MusicPlayer player)
            {
                var obj = comboBox1.SelectedItem as StoryboardObject;
                var object_info_window = ToolManager.GetTool<ObjectVisualizerDebugger>()?.Window;

                if (object_info_window!=null&&obj!=null)
                    object_info_window.SelectObject=obj;

                player.Jump((float)hitsound.Value.Time, true);
            }
        }

        public void ExcplictSelect(StoryboardObject obj)
        {
            Debug.Assert(comboBox1.Items.OfType<StoryboardObject>().Contains(obj));

            comboBox1.SelectedItem=obj;
        }

        private void TriggerConditionViewerWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel=true;
            Hide();
        }
    }
}