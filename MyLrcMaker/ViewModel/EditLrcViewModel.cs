using System;
using MyLrcMaker.Infrastructure;
using MyLrcMaker.Model;

namespace MyLrcMaker.ViewModel
{
    public class EditLrcViewModel : ResultViewModel<ILrcModel>
    {
        public ILrcModel LrcModel { get; }

        public int Minutes
        {
            get => _minutes;
            set => SetProperty(ref _minutes, value);
        }

        public int Seconds
        {
            get => _seconds;
            set => SetProperty(ref _seconds, value);
        }

        public int Milliseconds
        {
            get => _milliseconds;
            set => SetProperty(ref _milliseconds, value);
        }

        public EditLrcViewModel(ILrcModel lrcModel)
        {
            LrcModel = lrcModel;
            Minutes = LrcModel.Time.Minutes;
            Seconds = LrcModel.Time.Seconds;
            Milliseconds = LrcModel.Time.Milliseconds;
        }

        public override void Dispose()
        {
            LrcModel.Time = new TimeSpan(0, 0, Minutes, Seconds, Milliseconds);
            Result = LrcModel;
        }

        #region Fields

        private int _minutes;
        private int _seconds;
        private int _milliseconds;

        #endregion
    }
}