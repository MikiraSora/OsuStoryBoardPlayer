﻿using ReOsuStoryboardPlayer.Core.Base;
using ReOsuStoryboardPlayer.Core.Utils;
using ReOsuStoryboardPlayer.Tools.DefaultTools.ObjectInfoVisualizer;
using ReOsuStoryboardPlayer.Kernel;
using ReOsuStoryboardPlayer.Player;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ReOsuStoryboardPlayer.Tools.DefaultTools.ObjectsSequenceViewer
{
    public partial class ObjectsSequenceViewer : Form
    {
        private Dictionary<ListViewItem, StoryboardObject> registed_map = new Dictionary<ListViewItem, StoryboardObject>();

        private Regex current_filter = null;

        public ObjectsSequenceViewer()
        {
            InitializeComponent();

            var width_unit = (listView1.Width)/10.0f;

            ColumnHeader index_header = new ColumnHeader
            {
                Text="Object",
                Width=(int)(width_unit*9),
                TextAlign=HorizontalAlignment.Center
            };

            ColumnHeader object_header = new ColumnHeader
            {
                Text="Index",
                Width=(int)(width_unit*1),
                TextAlign=HorizontalAlignment.Center
            };

            listView1.Columns.Add(index_header);
            listView1.Columns.Add(object_header);
            listView1.Alignment=ListViewAlignment.Left;
        }

        private IEnumerable<StoryboardObject> RangeObjects;

        public void ApplyRangeFlush(Range range)
        {
            var objects = StoryboardInstanceManager.ActivityInstance.Updater.StoryboardObjectList
                .Where(o => range.InRange(o.FrameStartTime)||range.InRange(o.FrameEndTime));

            RangeObjects=objects;
            ApplyObjectsFlush();
        }

        public void ApplyObjectsFlush()
        {
            var time = MusicPlayerManager.ActivityPlayer?.CurrentTime??0;

            foreach (ListViewItem item in listView1.Items)
                ObjectPool<ListViewItem>.Instance.PutObject(item);

            listView1.Items.Clear();
            registed_map.Clear();

            var items = RangeObjects
                .Where(o => current_filter?.IsMatch(o.ToString())??true)
                .Select(o =>
                {
                    ListViewItem item = ObjectPool<ListViewItem>.Instance.GetObject();

                    item.ForeColor=(o.FrameStartTime<=time&&time<=o.FrameEndTime) ? Color.Black : Color.Gray;
                    item.Text=o.ToString();
                    item.SubItems.Add(o.Z.ToString());

                    registed_map[item]=o;
                    return item;
                });

            listView1.BeginUpdate();
            foreach (var item in items)
                listView1.Items.Add(item);
            listView1.EndUpdate();
        }

        private static Regex reg = new Regex(@"^([\w\(\)]+)(([+-]{1,2})(\d+))*$");

        private void ParseFlush()
        {
            var expression = textBox1.Text.Trim();

            var match = reg.Match(expression);

            if (!match.Success)
            {
                textBox1.ForeColor=Color.Red;
                return;
            }

            textBox1.ForeColor=Color.Black;

            try
            {
                var base_time = match.Groups[1].Value.Replace("now()", MusicPlayerManager.ActivityPlayer.CurrentTime.ToString()).ToInt();

                if (!string.IsNullOrWhiteSpace(match.Groups[2].Value))
                {
                    var sign = match.Groups[3].Value;
                    var offset = match.Groups[4].Value.ToInt();

                    Range range = new Range() { Start=base_time, End=base_time };

                    if ((!sign.All(c => c=='+'||c=='-'))||sign.Distinct().Count()!=sign.Length)
                    {
                        textBox1.ForeColor=Color.Red;
                        return;
                    }
                    else if (sign.Contains('+'))
                        range.End+=offset;
                    else if (sign.Contains('-'))
                        range.Start-=offset;

                    ApplyRangeFlush(range);
                }
            }
            catch
            {
                textBox1.ForeColor=Color.Red;
            }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData.HasFlag(Keys.Enter))
            {
                e.Handled=true;
                ParseFlush();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ParseFlush();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            textBox1.ForeColor=Color.Black;
        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var visualizer = ToolManager.GetTool<ObjectVisualizerDebugger>();
            var select_item = listView1.SelectedItems.OfType<ListViewItem>().FirstOrDefault();

            if (visualizer==null||select_item==null)
                return;

            var select_object = registed_map[select_item];

            visualizer.Window.SelectObject=select_object;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            RangeObjects=StoryboardInstanceManager.ActivityInstance.Updater.UpdatingStoryboardObjects.OrderBy(c => c.Z);
            ApplyObjectsFlush();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                current_filter=new Regex(textBox2.Text);
                ApplyObjectsFlush();
            }
            catch
            {
                textBox2.ForeColor=Color.Red;
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            textBox2.ForeColor=Color.Black;
        }
    }
}