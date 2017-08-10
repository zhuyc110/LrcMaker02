using System.ComponentModel;
using System.Windows.Controls;

namespace MyLrcMaker.Infrastructure
{
    public interface ISongService : INotifyPropertyChanged
    {
        double TotalLength { get; }

        double Current { get; }

        void Initialize(MediaElement mediaElement);

        void Halt();

        void Resume();

        void Release();
    }
}