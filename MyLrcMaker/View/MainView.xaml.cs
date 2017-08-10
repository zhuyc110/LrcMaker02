using System.ComponentModel.Composition;
using System.Windows;
using MyLrcMaker.Infrastructure;
using MyLrcMaker.ViewModel;

namespace MyLrcMaker.View
{
    /// <summary>
    /// MainView.xaml 的交互逻辑
    /// </summary>
    [Export(typeof(MainView))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public partial class MainView : IView<MainViewModel>
    {
        public MainViewModel ViewModel
        {
            set => DataContext = value;
            get => (MainViewModel) DataContext;
        }

        public string Title => "Main";

        [ImportingConstructor]
        public MainView(MainViewModel mainViewModel, ISongService songService)
        {
            _songService = songService;
            InitializeComponent();

            ViewModel = mainViewModel;
            ViewModel.OnMediaPlayerStatusChange += ViewModelOnMediaPlayerStatusChange;
        }

        #region Private methods

        private void ViewModelOnMediaPlayerStatusChange(object sender, MediaPlayerStatusChangeArgs e)
        {
            switch (e.Command)
            {
                case MediaPlayerCommand.Play:
                    _songService.Initialize(MediaElementPlayer);
                    MediaElementPlayer.Play();
                    break;
                case MediaPlayerCommand.Pause:
                    _songService.Halt();
                    MediaElementPlayer.Pause();
                    break;
                case MediaPlayerCommand.Stop:
                    _songService.Release();
                    MediaElementPlayer.Stop();
                    break;
            }
        }

        #endregion

        #region Fields

        private readonly ISongService _songService;

        #endregion
    }
}