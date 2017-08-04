using System.ComponentModel.Composition;
using System.Windows;
using MyLrcMaker.Infrastructure;
using MyLrcMaker.ViewModel;

namespace MyLrcMaker.View
{
    /// <summary>
    /// EditLrcView.xaml 的交互逻辑
    /// </summary>
    [Export(typeof(EditLrcView))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class EditLrcView : IView<EditLrcViewModel>
    {
        public EditLrcViewModel ViewModel
        {
            set => DataContext = value;
            get => (EditLrcViewModel) DataContext;
        }

        public string Title => "编辑时间";

        public EditLrcView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        #region Private methods

        private void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }
            ViewModel?.Dispose();
            _isDisposed = true;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Unloaded += OnUnloaded;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnLoaded;
            Unloaded -= OnUnloaded;
            Dispose();
        }

        #endregion

        #region Fields

        private bool _isDisposed;

        #endregion
    }
}