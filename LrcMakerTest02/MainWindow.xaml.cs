﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;

namespace LrcMakerTest02
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            timer.Tick += new EventHandler(TickEvent);
            timer.Start();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateListBox();
        }

        LrcManager lrcManager = new LrcManager();
        DispatcherTimer timer;

        string MusicFileName = string.Empty;
        private void OpenMusicFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "音频文件|*.mp3;*.wav;*.ape;*.aac;*.ogg;*.wma";
            dialog.Multiselect = false;
            dialog.Title = "打开音频文件";
            DialogResult result = dialog.ShowDialog();
            if (result != System.Windows.Forms.DialogResult.OK)
            {
                dialog.Dispose();
                return;
            }
            MusicFileName = dialog.FileName;
            MediaPlayer.Source = new Uri(MusicFileName);
            // 检查是否有本地歌词
            //string lrcPath = Path.GetFileNameWithoutExtension(dialog.FileName) + ".lrc";
            //dialog.Dispose();
            //if (!File.Exists(lrcPath)) return;
            //var result2 = System.Windows.MessageBox.Show("发现本地关联歌词，是否导入？");
            //if (result != System.Windows.Forms.DialogResult.OK) return;
        }

        private void Play(object sender, RoutedEventArgs e)
        {
            MediaPlayer.Play();
        }
        private void Pause(object sender, RoutedEventArgs e)
        {
            MediaPlayer.Pause();
        }
        private void Stop(object sender, RoutedEventArgs e)
        {
            MediaPlayer.Stop();
        }
        private void Forward(object sender, RoutedEventArgs e)
        {
            MediaPlayer.Position += new TimeSpan(0, 0, 5);
        }
        private void Backward(object sender, RoutedEventArgs e)
        {
            MediaPlayer.Position -= new TimeSpan(0, 0, 5);
        }

        void TickEvent(object sender, EventArgs e)
        {
            CurrentPosition.Text = Time.Parse(CurrentTime).Info;

            // 播放并测试
            if (PlayAndTest.IsChecked.Value)
            {
                double temptime = CurrentTime;
                List<Lrc> templist = lrcManager.LrcList;
                double diff = Time.Max.TotalSeconds;
                int index = 0;
                for (int i = 0; i < templist.Count; i++)
                {
                    double diff1 = temptime - templist[i].ActualTime.TotalSeconds;
                    if (diff1 > 0 && diff1 < diff)
                    {
                        diff = diff1;
                        index = i;
                    }
                }
                Index = index;
            }
        }

        private void MediaPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            TotalLength.Text = Time.Parse(MediaPlayer.NaturalDuration.TimeSpan).Info;
        }
        private void OpenLrcFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "歌词文件|*.txt;*.lrc";
            dialog.Multiselect = false;
            dialog.Title = "打开歌词文件";
            DialogResult result = dialog.ShowDialog();
            if (result != System.Windows.Forms.DialogResult.OK)
            {
                dialog.Dispose();
                return;
            }
            lrcManager.LoadFromFileName(dialog.FileName);
            UpdateListBox();
        }
        private void ImportLrcFromClipboard(object sender, RoutedEventArgs e)
        {
            string content = System.Windows.Clipboard.GetText();
            lrcManager.LoadFromString(content);
            UpdateListBox();
        }
        private void OpenLrcFileWithoutTime(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "歌词文件|*.txt;*.lrc";
            dialog.Multiselect = false;
            dialog.Title = "打开歌词文件";
            DialogResult result = dialog.ShowDialog();
            if (result != System.Windows.Forms.DialogResult.OK)
            {
                dialog.Dispose();
                return;
            }
            lrcManager.LoadFromFileNameWithoutTime(dialog.FileName);
            UpdateListBox();
        }
        private void ImportLrcFromClipboardWithoutTime(object sender, RoutedEventArgs e)
        {
            string content = System.Windows.Clipboard.GetText();
            lrcManager.LoadFromStringWithoutTime(content);
            UpdateListBox();
        }
        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void SaveLrcFile(object sender, RoutedEventArgs e)
        {
            string LrcFileName = string.Empty;
            if (!string.IsNullOrEmpty(MusicFileName))
                LrcFileName = Path.GetFileNameWithoutExtension(MusicFileName) + ".lrc";
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Title = "保存歌词文件";
            dialog.Filter = "歌词文件|*.lrc";
            dialog.FileName = LrcFileName;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                StreamWriter sw = new StreamWriter(dialog.FileName);
                sw.Write(lrcManager.ContentInLines);
                sw.Dispose();
            }
        }

        void UpdateListBox()
        {
            LrcListBox.Items.Clear();
            List<string> tempList = lrcManager.StringList;
            foreach (string temp in tempList)
            {
                LrcListBox.Items.Add(temp);
            }
        }
        void UpdateListBox(int index)
        {
            UpdateListBox();
            Index = index;
        }

        private int Index
        {
            get { return LrcListBox.SelectedIndex; }
            set
            {
                LrcListBox.SelectedIndex = value;
                LrcListBox.ScrollIntoView(LrcListBox.SelectedItem);
            }
        }
        private double CurrentTime
        {
            get { return MediaPlayer.Position.TotalMilliseconds / 1000; }
            set { MediaPlayer.Position = new TimeSpan(0, 0, 0, 0, (int)(value * 1000)); }
        }

        // 上方按钮
        private void InsertNewLine(object sender, RoutedEventArgs e)
        {
            int tempIndex = Index + 1;
            lrcManager.AddLineAt(tempIndex, new Lrc(CurrentTime, ""));
            UpdateListBox(tempIndex);
            LrcBox.Focus();
            MediaPlayer.Pause();
        }
        private void InsertLine(object sender, RoutedEventArgs e)
        {
            if (Index > -1)
            {
                int tempIndex = Index;
                lrcManager.AddLineAt(tempIndex, new Lrc(CurrentTime, ""));
                UpdateListBox(tempIndex);
            }
            else if (Index == -1)
            {
                int tempIndex = 0;
                lrcManager.AddLineAt(tempIndex, new Lrc(CurrentTime, ""));
                UpdateListBox(tempIndex);
            }
        }
        private void DeleteLine(object sender, RoutedEventArgs e)
        {
            if (Index == -1) return;
            else if (Index < LrcListBox.Items.Count - 1)
            {
                int tempIndex = Index;
                lrcManager.RemoveLineAt(tempIndex);
                UpdateListBox(tempIndex);
            }
            else if (Index == LrcListBox.Items.Count - 1)
            {
                int tempIndex = Index;
                lrcManager.RemoveLineAt(tempIndex);
                UpdateListBox(tempIndex - 1);
            }
        }
        private void MoveLineUp(object sender, RoutedEventArgs e)
        {
            if (Index == 0) return;
            int tempIndex = Index;
            lrcManager.MoveLineUp(Index);
            UpdateListBox(tempIndex - 1);
        }
        private void MoveLineDown(object sender, RoutedEventArgs e)
        {
            if (Index == LrcListBox.Items.Count - 1) return;
            int tempIndex = Index;
            lrcManager.MoveLineDown(Index);
            UpdateListBox(tempIndex + 1);
        }
        private void Cancel(object sender, RoutedEventArgs e)
        {
            // 暂时先不做历史记录功能
        }
        private void ClearAll(object sender, RoutedEventArgs e)
        {
            lrcManager.Clear();
            UpdateListBox();
        }

        // 菜单按钮
        private void InsertInfo(object sender, RoutedEventArgs e)
        {

        }

        // 根据LrcListBox的选择，更新下方两个文本框的内容
        void ShowCurrentLineInfo()
        {
            if (Index == -1) return;
            Lrc lrc = lrcManager.GetLineAt(Index);
            TimeBox.Text = lrc.ActualTime.Info;
            LrcBox.Text = lrc.LrcContent;
        }

        // 所有能触发文本框内容更新的事件
        private void LrcListBox_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ShowCurrentLineInfo();
        }
        private void LrcListBox_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ShowCurrentLineInfo();
            // 将来可以添加更多的右键单击事件
        }
        private void LrcListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ShowCurrentLineInfo();
        }

        private void AmendLine(object sender, RoutedEventArgs e)
        {
            int tempIndex = Index;
            lrcManager.AmendLineAt(Index, TimeBox.Text, LrcBox.Text);
            UpdateListBox(tempIndex);
        }
        private void AmendLineAtTime(object sender, RoutedEventArgs e)
        {
            // 如果时间偏差中的数字不科学，就不折腾了
            double result;
            if (!double.TryParse(TimeOffset.Text, out result)) return;
            int tempIndex = Index;
            lrcManager.AmendLineAt(Index, Time.Parse(CurrentTime + result / 1000).Info, LrcBox.Text);
            UpdateListBox();
            if (tempIndex >= LrcListBox.Items.Count - 1) Index = LrcListBox.Items.Count - 1;
            else Index = tempIndex + 1;
        }

        private void SortLrc(object sender, RoutedEventArgs e)
        {
            lrcManager.SortByTime();
            UpdateListBox();
        }
        private void SetAllZero(object sender, RoutedEventArgs e)
        {
            int tempIndex = Index;
            lrcManager.SetAllZero();
            UpdateListBox(tempIndex);
        }

        private void FinishNewLine(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                AmendLineAtTime(this, null);
                NewLineButton.Focus();
                MediaPlayer.Play();
            }
        }

        private void CopyLrcText(object sender, RoutedEventArgs e)
        {
            System.Windows.Clipboard.SetText(lrcManager.ContentInLines);
        }

        private void ListBoxItem_MouseDoubleClick(object sender, RoutedEventArgs e)
        {
            CurrentTime = lrcManager.GetLineAt(Index).ActualTime.TotalSeconds;
        }
    }
}