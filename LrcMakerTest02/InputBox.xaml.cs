using System.Windows;
using System.Windows.Input;

namespace LrcMakerTest02
{
    /// <summary>
    /// InputBox.xaml 的交互逻辑
    /// </summary>
    public partial class InputBox : Window
    {
        public InputBox()
        {
            InitializeComponent();
        }

        public static string Input(string HintText = "请输入：", string DefaultText = "",
            string Title = "提示", string ButtonText = "确认")
        {
            InputBox input = new InputBox();
            input.Title = Title;
            input.Notice.Text = HintText;
            input.Inputbox.Text = DefaultText;
            input.Confirm.Content = ButtonText;
            input.Inputbox.Focus();
            input.Inputbox.SelectAll();
            input.ShowDialog();
            return input.Inputbox.Text;
        }

        #region Private methods

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Input_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Close();
            }
            else if (e.Key == Key.Escape)
            {
                Inputbox.Text = "";
                Close();
            }
        }

        #endregion
    }
}