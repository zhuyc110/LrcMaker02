using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Windows.Forms;
using System.Windows.Input;
using MyLrcMaker.Extension;
using MyLrcMaker.Infrastructure;
using Prism.Commands;
using Prism.Mvvm;

namespace MyLrcMaker.ViewModel
{
    [Export(typeof(MainViewModel))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class MainViewModel : BindableBase
    {
        public event EventHandler<MediaPlayerStatusChangeArgs> OnMediaPlayerStatusChange;

        public LrcBoardViewModel LrcBoardViewModel { get; }

        public ICommand OpenMediaCommand { get; }

        public ICommand PlayMediaCommand { get; }

        public ICommand PauseMediaCommand { get; }

        public Uri MediaSource
        {
            get => _mediaSource;
            set
            {
                if (SetProperty(ref _mediaSource, value))
                {
                    ForceUpdateCanExecuteCommands();
                }
            }
        }

        public double TotalLength
        {
            get => _totalLength;
            set => SetProperty(ref _totalLength, value);
        }

        public double Current
        {
            get => _current;
            set
            {
                if (SetProperty(ref _current, value))
                {
                    if (_songService.Current != Current)
                    {
                        _songService.Resume();
                    }
                }
                
            }
        }

        [ImportingConstructor]
        public MainViewModel(LrcBoardViewModel lrcBoardViewModel, ISongService songService)
        {
            _songService = songService;
            _songService.PropertyChanged += OnSongServicePropertyChanged;
            LrcBoardViewModel = lrcBoardViewModel;
            OpenMediaCommand = new DelegateCommand(OpenMediaFile);
            PlayMediaCommand = new DelegateCommand<MediaPlayerCommand?>(RaiseMediaPlayerStatusChange, CanPlay);
            PauseMediaCommand = new DelegateCommand<MediaPlayerCommand?>(RaiseMediaPlayerStatusChange, CanPause);
        }

        #region Private methods

        private bool CanPause(MediaPlayerCommand? command)
        {
            return MediaSource != null && command.HasValue && IsMediaCommandLegal(command.Value);
        }

        private bool CanPlay(MediaPlayerCommand? command)
        {
            return MediaSource != null && command.HasValue && IsMediaCommandLegal(command.Value);
        }

        private void ForceUpdateCanExecuteCommands()
        {
            PlayMediaCommand.ForceUpdateCanExecuteCommand();
            PauseMediaCommand.ForceUpdateCanExecuteCommand();
        }

        private bool IsMediaCommandLegal(MediaPlayerCommand command)
        {
            if (_preCommand == MediaPlayerCommand.Play && command == MediaPlayerCommand.Play)
            {
                return false;
            }
            if ((_preCommand == MediaPlayerCommand.Pause || _preCommand == MediaPlayerCommand.Stop) && command == MediaPlayerCommand.Pause)
            {
                return false;
            }

            return true;
        }

        private void OnSongServicePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ISongService.TotalLength))
            {
                TotalLength = _songService.TotalLength;
            }

            if (e.PropertyName == nameof(ISongService.Current))
            {
                Current = _songService.Current;
            }
        }

        private void OpenMediaFile()
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Title = "打开音频文件";
                dialog.Multiselect = false;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    MediaSource = new Uri(dialog.FileName);
                }
            }
        }

        private void RaiseMediaPlayerStatusChange(MediaPlayerCommand? command)
        {
            Debug.Assert(command != null, "command can not be null");
            OnMediaPlayerStatusChange?.Invoke(this, new MediaPlayerStatusChangeArgs(command.Value));
            _preCommand = command.Value;
            ForceUpdateCanExecuteCommands();
        }

        #endregion

        #region Fields

        private readonly ISongService _songService;

        private Uri _mediaSource;
        private MediaPlayerCommand _preCommand = MediaPlayerCommand.Stop;
        private double _totalLength;
        private double _current;

        #endregion
    }
}