using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace LrcMakerTest02
{
    class LrcManager
    {
        private List<Lrc> lrcList = new List<Lrc>();
        public LrcManager()
        {
            Clear();
        }
        public void LoadFromString(string content)
        {
            Clear();
            string[] list = content.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            // 逐行添加到LrcList中，并检查是否为空行（包括只有空格的行）
            Regex emptyCheck = new Regex(@"\s*");
            foreach (string temp in list)
            {
                // 如果空白的长度与整行内容的长度相同，则表明为空白行
                if (emptyCheck.Match(temp).Length == temp.Length)
                    continue;
                else
                {
                    Lrc l = new Lrc(temp);
                    lrcList.Add(l);
                }
            }
        }
        public void LoadFromStringWithoutTime(string content)
        {
            Clear();
            string[] list = content.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            // 逐行添加到LrcList中，并检查是否为空行（包括只有空格的行）
            Regex emptyCheck = new Regex(@"\s*");
            foreach (string temp in list)
            {
                // 如果空白的长度与整行内容的长度相同，则表明为空白行
                if (emptyCheck.Match(temp).Length == temp.Length)
                    continue;
                else
                {
                    Lrc l = new Lrc("[00:00.000]" + temp);
                    lrcList.Add(l);
                }
            }
        }
        public void LoadFromFileName(string path)
        {
            Clear();
            // 假设这里的path是通过OpenFileDialog获取的，文件一定存在
            StreamReader reader = new StreamReader(path);
            LoadFromString(reader.ReadToEnd());
            reader.Dispose();
        }
        public void LoadFromFileNameWithoutTime(string path)
        {
            Clear();
            StreamReader reader = new StreamReader(path);
            LoadFromStringWithoutTime(reader.ReadToEnd());
            reader.Dispose();
        }
        public void AddLine(Lrc line)
        {
            lrcList.Add(line);
        }
        public void AddLineAt(int index, Lrc line)
        {
            if (index < 0 || index > lrcList.Count) return;
            lrcList.Insert(index, line);
        }
        public void RemoveLineAt(int index)
        {
            if (index < 0 || index >= lrcList.Count) return;
            else lrcList.RemoveAt(index);
        }
        public void AmendLineAt(int index, string time, string content)
        {
            if (index < 0 || index >= lrcList.Count) return;
            if (!Time.TryParse(time)) return;
            lrcList[index].ActualTime = Time.Parse(time);
            lrcList[index].LrcContent = content;
        }
        public void Clear()
        {
            lrcList.Clear();
        }
        public void MoveLineUp(int index)
        {
            if (index <= 0 || index >= lrcList.Count) return;
            Lrc temp = GetLineAt(index);
            RemoveLineAt(index);
            AddLineAt(index - 1, temp);
        }
        public void MoveLineDown(int index)
        {
            if (index < 0 || index >= lrcList.Count - 1) return;
            Lrc temp = GetLineAt(index);
            RemoveLineAt(index);
            AddLineAt(index + 1, temp);
        }
        public Lrc GetLineAt(int index)
        {
            if (index < 0) return lrcList[0];
            else if (index >= lrcList.Count) return lrcList[lrcList.Count - 1];
            else return lrcList[index];
        }
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
        public List<Lrc> LrcList
        {
            get
            {
                return lrcList;
            }
        }

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
        public void SortByTime()
        {
            lrcList.Sort();
        }
        public void SetAllZero()
        {
            foreach(Lrc lrc in lrcList)
            {
                lrc.ActualTime = new Time(0);
            }
        }
    }
}
