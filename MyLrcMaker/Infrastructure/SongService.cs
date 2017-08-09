using System;
using System.ComponentModel.Composition;
using System.Windows.Controls;
using System.Windows.Threading;
using Prism.Mvvm;

namespace MyLrcMaker.Infrastructure
{
    [Export(typeof(ISongService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class SongService : BindableBase, ISongService
    {
        public double TotalLength
        {
            get => _totalLength;
            set => SetProperty(ref _totalLength, value);
        }

        public double Current
        {
            get => _current;
            set => SetProperty(ref _current, value);
        }

        #region ISongService Members

        public void Initialize(MediaElement mediaElement)
        {
            if (Equals(_mediaElement, mediaElement))
            {
                return;
            }
            _mediaElement = mediaElement;
            _mediaElement.MediaOpened += OnMediaOpened;
        }

        private void OnMediaOpened(object sender, System.Windows.RoutedEventArgs e)
        {
            Release();

            TotalLength = _mediaElement.NaturalDuration.TimeSpan.TotalMilliseconds;
            Current = 0;
            _timer = new DispatcherTimer(TimeSpan.FromMilliseconds(TotalLength / 100), DispatcherPriority.Normal, TimerTickCallBack, Dispatcher.CurrentDispatcher);
            _timer.Start();
        }

        public void Release()
        {
            if (_timer != null && _timer.IsEnabled)
            {
                _timer.Stop();
            }
        }

        #endregion

        #region Private methods

        private void TimerTickCallBack(object sender, EventArgs e)
        {
            Current = _mediaElement.Position.TotalMilliseconds;
        }

        #endregion

        #region Fields

        private MediaElement _mediaElement;
        private DispatcherTimer _timer;
        private double _totalLength;
        private double _current;

        #endregion
    }
}