using MyLrcMaker.Model;
using Prism.Mvvm;

namespace MyLrcMaker.ViewModel
{
    public class EditLrcViewModel : BindableBase
    {
        public ILrcModel LrcModel { get; }

        public EditLrcViewModel(ILrcModel lrcModel)
        {
            LrcModel = lrcModel;
        }
    }
}