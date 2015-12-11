using System;
using System.Text.RegularExpressions;

namespace LrcMakerTest02
{
    class Lrc : IComparable
    {
        public string LrcContent = string.Empty;
        public Time ActualTime = Time.Empty;
        public Lrc(double time, string content)
        {
            LrcContent = content;
            ActualTime = Time.Parse(time);
        }
        public Lrc(Time time, string content)
        {
            LrcContent = content;
            ActualTime = new Time(time);
        }
        public Lrc(string line)
        {
            Regex time = new Regex(@"(?<=\[).*?(?=\])");
            Regex content = new Regex(@"(?<=\]).*$");
            if (!Time.TryParse(time.Match(line).ToString()))
                throw new Exception("歌词内容有误，无法获取时间");
            else
            {
                string t = time.Match(line).ToString();
                LrcContent = content.Match(line).ToString();
                ActualTime = Time.Parse(t);
            }
        }
        public Lrc()
        {
            // Nothing
        }
        public string Info
        {
            get
            {
                return "[" + ActualTime.Info + "]" + LrcContent;
            }
        }

        public int CompareTo(object obj)
        {
            Lrc o = (Lrc)obj;
            if (ActualTime > o.ActualTime) return 1;
            else if (ActualTime.TotalSeconds == o.ActualTime.TotalSeconds) return 0;
            else return -1;
        }
    }
}
