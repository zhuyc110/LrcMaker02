using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
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
            TimeBox.Clear();
            LrcBox.Clear();

            if (File.Exists("temp.txt"))
            {
                lrcManager.LoadFromFileName("temp.txt");
                UpdateListBox();
            }
        }
        private void MainForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (lrcManager.LrcList.Count > 0)
            {
                if (System.Windows.MessageBox.Show("是否将当前的歌词文本保存为本地临时文件（temp.txt）？", "提示", MessageBoxButton.OKCancel)
                    == System.Windows.MessageBoxResult.OK)
                {
                    StreamWriter sw = new StreamWriter("temp.txt");
                    sw.Write(lrcManager.ContentInLines);
                    sw.Close();
                    sw.Dispose();
                }
                else
                {
                    if(File.Exists("temp.txt"))
                    {
                        File.Delete("temp.txt");
                    }
                }
            }
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

        private bool isPlaying = false;
        private void Play(object sender, RoutedEventArgs e)
        {
            if (MediaPlayer.Source == null) return;
            System.Windows.Controls.Button b = PlayPause;
            if ((string)b.Content == "播放")
            {
                MediaPlayer.Play();
                b.Content = "暂停";
                b.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFECFF74"));
                isPlaying = true;
            }
            else
            {
                MediaPlayer.Pause();
                b.Content = "播放";
                b.Background = new SolidColorBrush(Colors.LightGreen);
                isPlaying = false;
            }
        }
        private void Stop(object sender, RoutedEventArgs e)
        {
            MediaPlayer.Stop();
            PlayPause.Content = "播放";
            isPlaying = false;
        }
        private void Forward5(object sender, RoutedEventArgs e)
        {
            MediaPlayer.Position += new TimeSpan(0, 0, 5);
        }
        private void Backward5(object sender, RoutedEventArgs e)
        {
            MediaPlayer.Position -= new TimeSpan(0, 0, 5);
        }
        private void Forward1(object sender, RoutedEventArgs e)
        {
            MediaPlayer.Position += new TimeSpan(0, 0, 1);
        }
        private void Backward1(object sender, RoutedEventArgs e)
        {
            MediaPlayer.Position -= new TimeSpan(0, 0, 1);
        }

        void TickEvent(object sender, EventArgs e)
        {
            CurrentPosition.Text = Time.Parse(CurrentTime).Info;

            // 播放并测试
            if (PlayAndTest.IsChecked.Value && LrcListBox.Items.Count > 0 && isPlaying)
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

        // 菜单按钮
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
            dialog.Dispose();
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
                sw.Close();
                sw.Dispose();
            }
        }
        private void InsertInfo(object sender, RoutedEventArgs e)
        {

        }
        private void ShiftAll(object sender, RoutedEventArgs e)
        {
            string offsetText = InputBox.Input("请输入偏移时间（秒）：");
            if (string.IsNullOrEmpty(offsetText)) return;
            double offset;
            if (double.TryParse(offsetText, out offset))
            {
                if (Math.Abs(offset) <= 3600)
                {
                    lrcManager.ShiftAll(offset);
                    UpdateListBox();
                }
                else
                    System.Windows.MessageBox.Show("您输入的时间偏差超过1小时，系统认为您可能输入有误。");
            }
            else
                System.Windows.MessageBox.Show("您输入的数字有误，请重试。");
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

        // 一排六个按钮
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
                int tempIndex = Index + 1;
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
            // 不是最后一项，删除后选择后一项
            else if (Index < LrcListBox.Items.Count - 1)
            {
                int tempIndex = Index;
                lrcManager.RemoveLineAt(tempIndex);
                UpdateListBox(tempIndex);
            }
            // 是最后一项，删除后选择前一项
            else if (Index == LrcListBox.Items.Count - 1)
            {
                int tempIndex = Index;
                lrcManager.RemoveLineAt(tempIndex);
                UpdateListBox(tempIndex - 1);
            }
            if (LrcListBox.Items.Count == 0)
            {
                LrcBox.Text = "";
                TimeBox.Text = "";
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
        private void Help_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.MenuItem mi = sender as System.Windows.Controls.MenuItem;
            if ((string)mi.Header == "帮助")
            {
                if (!File.Exists("Help.txt"))
                {
                    System.Windows.MessageBox.Show("找不到帮助文件。");
                }
                else
                {
                    Process.Start("Help.txt");
                }
            }
            else if ((string)mi.Header == "版本")
            {
                if (!File.Exists("Plan.txt"))
                {
                    System.Windows.MessageBox.Show("找不到帮助文件。");
                }
                else
                {
                    Process.Start("Plan.txt");
                }
            }
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
        private void CopyLrcText(object sender, RoutedEventArgs e)
        {
            System.Windows.Clipboard.SetText(lrcManager.ContentInLines);
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

        // 左下两个按钮
        private void AmendLine(object sender, RoutedEventArgs e)
        {
            int tempIndex = Index;
            lrcManager.AmendLineAt(Index, TimeBox.Text, LrcBox.Text);
            UpdateListBox(tempIndex);
        }
        private void AmendLineAtTime(object sender, RoutedEventArgs e)
        {
            double result;
            if (!double.TryParse(TimeOffset.Text, out result)) result = 0;
            int tempIndex = Index;
            lrcManager.AmendLineAt(Index, Time.Parse(CurrentTime + result / 1000).Info, LrcBox.Text);
            UpdateListBox();
            if (tempIndex >= LrcListBox.Items.Count - 1) Index = LrcListBox.Items.Count - 1;
            else Index = tempIndex + 1;
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

        private void ListBoxItem_MouseDoubleClick(object sender, RoutedEventArgs e)
        {
            CurrentTime = lrcManager.GetLineAt(Index).ActualTime.TotalSeconds;
        }
        private void AdjustLineTime(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (!Time.TryParse(TimeBox.Text)) return;
            Time newtime = Time.Parse(TimeBox.Text);
            if (e.Delta > 0 && newtime + 0.05 < Time.Max)
            {
                TimeBox.Text = (Time.Parse(TimeBox.Text) + 0.05).ToString();
                AmendLine(this, null);
            }
            else if (newtime - 0.05 > Time.Zero)
            {
                TimeBox.Text = (Time.Parse(TimeBox.Text) - 0.05).ToString();
                AmendLine(this, null);
            }
        }

    }
}
