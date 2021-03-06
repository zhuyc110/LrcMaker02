﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace LrcMakerTest02
{
    internal class LrcManager
    {
        /// <summary>
        /// 将List中所有内容转变为用于输出歌词文件的字符串链表
        /// </summary>
        public List<string> StringList
        {
            get
            {
                List<string> list = new List<string>();
                foreach (Lrc lrc in lrcList)
                {
                    list.Add(lrc.Info);
                }
                return list;
            }
        }

        /// <summary>
        /// 获取List，不可修改
        /// </summary>
        public List<Lrc> LrcList
        {
            get { return lrcList; }
        }

        /// <summary>
        /// 将List中所有内容转变为用于输出歌词文件的多行文本
        /// </summary>
        public string ContentInLines
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                int Length = StringList.Count;
                List<string> list = StringList;
                for (int i = 0; i < Length - 1; i++)
                {
                    sb.Append(list[i]);
                    sb.Append(Environment.NewLine);
                }
                sb.Append(list[Length - 1]);
                return sb.ToString();
            }
        }

        public LrcManager()
        {
        }

        /// <summary>
        /// 在List的末尾添加新行
        /// </summary>
        /// <param name="line"></param>
        public void AddLine(Lrc line)
        {
            lrcList.Add(line);
        }

        /// <summary>
        /// 在List的某一位置添加新行
        /// </summary>
        /// <param name="index"></param>
        /// <param name="line"></param>
        public void AddLineAt(int index, Lrc line)
        {
            if (index < 0 || index > lrcList.Count) return;
            lrcList.Insert(index, line);
        }

        /// <summary>
        /// 修改List中的某一行
        /// </summary>
        /// <param name="index"></param>
        /// <param name="time"></param>
        /// <param name="content"></param>
        public void AmendLineAt(int index, string time, string content)
        {
            if (index < 0 || index >= lrcList.Count) return;
            if (!Time.TryParse(time)) return;
            lrcList[index].ActualTime = Time.Parse(time);
            lrcList[index].LrcContent = content;
        }

        /// <summary>
        /// 清空List
        /// </summary>
        public void Clear()
        {
            lrcList.Clear();
        }

        /// <summary>
        /// 返回某一行对应的Lrc
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Lrc GetLineAt(int index)
        {
            if (index < 0) return lrcList[0];
            else if (index >= lrcList.Count) return lrcList[lrcList.Count - 1];
            else return lrcList[index];
        }

        /// <summary>
        /// 读取一个外部歌词文本，并解析为多个Lrc元素
        /// 歌词文本中包含时间信息
        /// </summary>
        /// <param name="path"></param>
        public void LoadFromFileName(string path)
        {
            Clear();
            // 假设这里的path是通过OpenFileDialog获取的，文件一定存在
            StreamReader reader = new StreamReader(path);
            LoadFromString(reader.ReadToEnd());
            reader.Dispose();
        }

        /// <summary>
        ///
        /// 读取一个外部歌词文本，并解析为多个Lrc元素
        /// 歌词文本中不包含时间信息
        /// </summary>
        /// <param name="path"></param>
        public void LoadFromFileNameWithoutTime(string path)
        {
            Clear();
            StreamReader reader = new StreamReader(path);
            LoadFromStringWithoutTime(reader.ReadToEnd());
            reader.Dispose();
        }

        /// <summary>
        /// 将全部歌词文本逐行分析，解析为多个Lrc元素
        /// 歌词文本中包含时间信息
        /// </summary>
        /// <param name="content"></param>
        public void LoadFromString(string content)
        {
            Clear();
            string[] list = content.Split(new string[] {Environment.NewLine}, StringSplitOptions.None);
            foreach (string temp in list)
            {
                Regex timeCheck = new Regex(@"\[[^\]]+\]");
                Regex LrcCheck = new Regex(@"(?<=\])[^\[]*$");
                MatchCollection mc = timeCheck.Matches(temp);
                foreach (Match m in mc)
                {
                    string lrc = m.ToString() + LrcCheck.Match(temp).ToString();
                    lrcList.Add(new Lrc(lrc));
                }
                SortByTime();
            }
        }

        /// <summary>
        /// 将全部歌词文本逐行分析，解析为多个Lrc元素
        /// 歌词文本中不包含时间信息，只有多行的歌词
        /// </summary>
        /// <param name="content"></param>
        public void LoadFromStringWithoutTime(string content)
        {
            Clear();
            string[] list = content.Split(new string[] {Environment.NewLine}, StringSplitOptions.None);
            foreach (string temp in list)
            {
                Lrc l = new Lrc("[00:00.000]" + temp);
                lrcList.Add(l);
            }
        }

        /// <summary>
        /// 将某一行与其后一行互换
        /// 如果index小于第一元素位置，或大于等于最后一元素位置，则无变化
        /// </summary>
        /// <param name="index"></param>
        public void MoveLineDown(int index)
        {
            if (index < 0 || index >= lrcList.Count - 1) return;
            Lrc temp = GetLineAt(index);
            RemoveLineAt(index);
            AddLineAt(index + 1, temp);
        }

        /// <summary>
        /// 将某一行与其前一行互换
        /// 如果index小于第一元素位置，或大于最后一元素位置，则无变化
        /// </summary>
        /// <param name="index"></param>
        public void MoveLineUp(int index)
        {
            if (index <= 0 || index >= lrcList.Count) return;
            Lrc temp = GetLineAt(index);
            RemoveLineAt(index);
            AddLineAt(index - 1, temp);
        }

        /// <summary>
        /// 删除List中的某一行
        /// </summary>
        /// <param name="index"></param>
        public void RemoveLineAt(int index)
        {
            if (index < 0 || index >= lrcList.Count) return;
            else lrcList.RemoveAt(index);
        }

        /// <summary>
        /// 将所有歌词的时间重置为0
        /// </summary>
        public void SetAllZero()
        {
            foreach (Lrc lrc in lrcList)
            {
                lrc.ActualTime = new Time(0);
            }
        }

        /// <summary>
        /// 将所有歌词行的时间整体偏移
        /// </summary>
        /// <param name="offset"></param>
        public void ShiftAll(double offset)
        {
            // 这里假设输入的offset是合理的
            foreach (Lrc line in lrcList)
            {
                double temp = line.ActualTime.TotalSeconds + offset;
                if (temp < 0) temp = 0;
                line.ActualTime = new Time(temp);
            }
        }

        /// <summary>
        /// 将所有歌词按照时间顺序排列
        /// </summary>
        public void SortByTime()
        {
            lrcList.Sort();
        }

        #region Fields

        private readonly List<Lrc> lrcList = new List<Lrc>();

        #endregion
    }
}