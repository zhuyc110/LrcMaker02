using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Windows.Forms;
using System.Windows.Input;
using MyLrcMaker.Extension;
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

        [ImportingConstructor]
        public MainViewModel(LrcBoardViewModel lrcBoardViewModel)
        {
            LrcBoardViewModel = lrcBoardViewModel;
            OpenMediaCommand = new DelegateCommand(OpenMediaFile);
            PlayMediaCommand = new DelegateCommand<MediaPlayerCommand?>(RaiseMediaPlayerStatusChange, CanPlay);
        }

        private bool CanPlay(MediaPlayerCommand? command)
        {
            return MediaSource != null && command.HasValue && IsMediaCommandLegal(command.Value);
        }

        #region Private methods

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
            PlayMediaCommand.ForceUpdateCanExecuteCommand();
        }

        private bool IsMediaCommandLegal(MediaPlayerCommand command)
        {
            if (_preCommand == MediaPlayerCommand.Play && command == MediaPlayerCommand.Play)
            {
                return false;
            }

            return true;
        }

        private void ForceUpdateCanExecuteCommands()
        {
            PlayMediaCommand.ForceUpdateCanExecuteCommand();
        }

        #endregion

        #region Fields

        private Uri _mediaSource;
        private MediaPlayerCommand _preCommand = MediaPlayerCommand.Stop;

        #endregion
    }
}