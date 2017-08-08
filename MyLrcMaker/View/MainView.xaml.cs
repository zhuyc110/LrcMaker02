using System.ComponentModel.Composition;
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
            get => (MainViewModel)DataContext;
        }

        public string Title => "Main";

        [ImportingConstructor]
        public MainView(MainViewModel mainViewModel)
        {
            InitializeComponent();

            ViewModel = mainViewModel;
            ViewModel.OnMediaPlayerStatusChange += ViewModelOnMediaPlayerStatusChange;
        }

        private void ViewModelOnMediaPlayerStatusChange(object sender, MediaPlayerStatusChangeArgs e)
        {
            switch (e.Command)
            {
                case MediaPlayerCommand.Play:
                    MediaElementPlayer.Play();
                    break;
                case MediaPlayerCommand.Pause:
                    MediaElementPlayer.Pause();
                    break;
                case MediaPlayerCommand.Stop:
                    MediaElementPlayer.Stop();
                    break;
            }
        }
    }
}