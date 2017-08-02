using System;

namespace MyLrcMaker.Model
{
    public interface ILrcModel
    {
        TimeSpan Time { get; }

        string Text { get; }
    }
}