using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Button = System.Windows.Controls.Button;
using Clipboard = System.Windows.Clipboard;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MenuItem = System.Windows.Controls.MenuItem;
using MessageBox = System.Windows.MessageBox;

namespace LrcMakerTest02
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
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
            set { MediaPlayer.Position = new TimeSpan(0, 0, 0, 0, (int) (value * 1000)); }
        }

        public MainWindow()
        {
            InitializeComponent();

            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            timer.Tick += new EventHandler(TickEvent);
            timer.Start();
        }

        #region Private methods

        private void AdjustLineTime(object sender, MouseWheelEventArgs e)
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

        private void Backward1(object sender, RoutedEventArgs e)
        {
            MediaPlayer.Position -= new TimeSpan(0, 0, 2);
        }

        private void Backward5(object sender, RoutedEventArgs e)
        {
            MediaPlayer.Position -= new TimeSpan(0, 0, 5);
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

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void CopyLrcText(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(lrcManager.ContentInLines);
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

        private void FinishNewLine(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AmendLineAtTime(this, null);
                NewLineButton.Focus();
                MediaPlayer.Play();
            }
        }

        private void Forward1(object sender, RoutedEventArgs e)
        {
            MediaPlayer.Position += new TimeSpan(0, 0, 2);
        }

        private void Forward5(object sender, RoutedEventArgs e)
        {
            MediaPlayer.Position += new TimeSpan(0, 0, 5);
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            MenuItem mi = sender as MenuItem;
            if ((string) mi.Header == "帮助")
            {
                if (!File.Exists("Help.txt"))
                {
                    MessageBox.Show("找不到帮助文件。");
                }
                else
                {
                    Process.Start("Help.txt");
                }
            }
            else if ((string) mi.Header == "版本")
            {
                if (!File.Exists("Plan.txt"))
                {
                    MessageBox.Show("找不到帮助文件。");
                }
                else
                {
                    Process.Start("Plan.txt");
                }
            }
        }

        private void ImportLrcFromClipboard(object sender, RoutedEventArgs e)
        {
            string content = Clipboard.GetText();
            lrcManager.LoadFromString(content);
            UpdateListBox();
        }

        private void ImportLrcFromClipboardWithoutTime(object sender, RoutedEventArgs e)
        {
            string content = Clipboard.GetText();
            lrcManager.LoadFromStringWithoutTime(content);
            UpdateListBox();
        }

        private void InsertInfo(object sender, RoutedEventArgs e)
        {
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

        // 一排六个按钮
        private void InsertNewLine(object sender, RoutedEventArgs e)
        {
            int tempIndex = Index + 1;
            lrcManager.AddLineAt(tempIndex, new Lrc(CurrentTime, ""));
            UpdateListBox(tempIndex);
            LrcBox.Focus();
            MediaPlayer.Pause();
            PlayPause.Content = "播放";
        }

        private void ListBoxItem_MouseDoubleClick(object sender, RoutedEventArgs e)
        {
            CurrentTime = lrcManager.GetLineAt(Index).ActualTime.TotalSeconds;
        }

        // 所有能触发文本框内容更新的事件
        private void LrcListBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ShowCurrentLineInfo();
        }

        private void LrcListBox_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ShowCurrentLineInfo();
            // 将来可以添加更多的右键单击事件
        }

        private void LrcListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShowCurrentLineInfo();
        }

        private void MainForm_Closing(object sender, CancelEventArgs e)
        {
            if (lrcManager.LrcList.Count > 0)
            {
                if (MessageBox.Show("是否将当前的歌词文本保存为本地临时文件（temp.txt）？", "提示", MessageBoxButton.OKCancel)
                    == MessageBoxResult.OK)
                {
                    StreamWriter sw = new StreamWriter("temp.txt");
                    sw.Write(lrcManager.ContentInLines);
                    sw.Close();
                    sw.Dispose();
                }
                //else
                //{
                //    if (File.Exists("temp.txt"))
                //    {
                //        File.Delete("temp.txt");
                //    }
                //}
            }
        }

        private void MediaPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            TotalLength.Text = Time.Parse(MediaPlayer.NaturalDuration.TimeSpan).Info;
        }

        private void MoveLineDown(object sender, RoutedEventArgs e)
        {
            if (Index == LrcListBox.Items.Count - 1) return;
            int tempIndex = Index;
            lrcManager.MoveLineDown(Index);
            UpdateListBox(tempIndex + 1);
        }

        private void MoveLineUp(object sender, RoutedEventArgs e)
        {
            if (Index == 0) return;
            int tempIndex = Index;
            lrcManager.MoveLineUp(Index);
            UpdateListBox(tempIndex - 1);
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
            dialog.Dispose();
            MusicFileName = dialog.FileName;
            MediaPlayer.Source = new Uri(MusicFileName);
            MediaPlayer.Stop();
            //检查是否有本地歌词
            string lrcPath = Path.GetDirectoryName(dialog.FileName) + "//" + Path.GetFileNameWithoutExtension(dialog.FileName) + ".lrc";
            if (!File.Exists(lrcPath)) return;
            var result2 = MessageBox.Show("发现本地关联歌词，是否导入？");
            if (result2 != MessageBoxResult.OK) return;
            lrcManager.LoadFromFileName(lrcPath);
            UpdateListBox();
        }

        // 音乐控制按钮
        private void Play(object sender, RoutedEventArgs e)
        {
            if (MediaPlayer.Source == null) return;
            Button b = PlayPause;
            if ((string) b.Content == "播放")
            {
                MediaPlayer.Play();
                b.Content = "暂停";
                b.Background = new SolidColorBrush((Color) ColorConverter.ConvertFromString("#FFECFF74"));
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

        private void SetAllZero(object sender, RoutedEventArgs e)
        {
            int tempIndex = Index;
            lrcManager.SetAllZero();
            UpdateListBox(tempIndex);
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
                    MessageBox.Show("您输入的时间偏差超过1小时，系统认为您可能输入有误。");
            }
            else
                MessageBox.Show("您输入的数字有误，请重试。");
        }

        // 根据LrcListBox的选择，更新下方两个文本框的内容
        private void ShowCurrentLineInfo()
        {
            if (Index == -1) return;
            Lrc lrc = lrcManager.GetLineAt(Index);
            TimeBox.Text = lrc.ActualTime.Info;
            LrcBox.Text = lrc.LrcContent;
        }

        private void SortLrc(object sender, RoutedEventArgs e)
        {
            lrcManager.SortByTime();
            UpdateListBox();
        }

        private void Stop(object sender, RoutedEventArgs e)
        {
            MediaPlayer.Stop();
            PlayPause.Content = "播放";
            isPlaying = false;
        }

        private void TickEvent(object sender, EventArgs e)
        {
            CurrentPosition.Text = Time.Parse(CurrentTime).Info;
            if (MediaPlayer.NaturalDuration.HasTimeSpan)
            {
                var current = MediaPlayer.Position.TotalMilliseconds;
                var total = MediaPlayer.NaturalDuration.TimeSpan.TotalMilliseconds;
                var process = current / total;
                CurrentPositionBar.Width = CurrentPosition.ActualWidth * process;
                //LrcBox.Text = string.Format("{0}, {1}, {2}",current, total, process);
            }

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

        private void UpdateListBox()
        {
            LrcListBox.Items.Clear();
            List<string> tempList = lrcManager.StringList;
            foreach (string temp in tempList)
            {
                LrcListBox.Items.Add(temp);
            }
        }

        private void UpdateListBox(int index)
        {
            UpdateListBox();
            Index = index;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateListBox();
            TimeBox.Clear();
            LrcBox.Clear();
            CurrentPositionBar.Width = 0;

            if (File.Exists("temp.txt"))
            {
                lrcManager.LoadFromFileName("temp.txt");
                UpdateListBox();
            }
        }

        #endregion

        #region Fields

        private readonly LrcManager lrcManager = new LrcManager();
        private readonly DispatcherTimer timer;

        private bool isPlaying = false;

        private string MusicFileName = string.Empty;

        #endregion
    }
}