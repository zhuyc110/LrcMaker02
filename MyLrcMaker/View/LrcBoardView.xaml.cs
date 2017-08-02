using System.ComponentModel.Composition;
using MyLrcMaker.ViewModel;

namespace MyLrcMaker.View
{
    /// <summary>
    /// LrcBoardView.xaml 的交互逻辑
    /// </summary>
    [Export(typeof(LrcBoardView))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public partial class LrcBoardView
    {
        [Import]
        public LrcBoardViewModel ViewModel
        {
            set => DataContext = value;
        }

        public LrcBoardView()
        {
            InitializeComponent();
        }
    }
}